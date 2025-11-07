// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using ElevenLabs.Voices;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using Utilities.Encoding.OggVorbis;
using Utilities.Encoding.Wav;
using Utilities.WebRequestRest;

namespace ElevenLabs
{
    [Preserve]
    [Serializable]
    public sealed class VoiceClip : GeneratedClip
    {
        [Preserve]
        internal VoiceClip(string id, string text, Voice voice, AudioClip audioClip, string cachedPath = null)
            : base(id, text, audioClip, cachedPath)
        {
            this.voice = voice;
        }

        [Preserve]
        internal VoiceClip(string id, string text, Voice voice, NativeArray<byte> clipData, int sampleRate, string cachedPath = null)
            : base(id, text, clipData, sampleRate, cachedPath)
        {
            this.voice = voice;
        }

        [SerializeField]
        private Voice voice;

        [Preserve]
        public Voice Voice => voice;

        [Preserve]
        public TimestampedTranscriptCharacter[] TimestampedTranscriptCharacters { get; internal set; }

        internal async Task SaveAudioToCacheAsync(OutputFormat outputFormat, CacheFormat cacheFormat, CancellationToken cancellationToken)
        {
#if PLATFORM_WEBGL
            await Task.Yield();
            return null;
#else
            if (cacheFormat == CacheFormat.None) { return; }

            string extension;
            AudioType audioType;

            if (outputFormat is OutputFormat.MP3_44100_64 or OutputFormat.MP3_44100_96 or OutputFormat.MP3_44100_128 or OutputFormat.MP3_44100_192)
            {
                extension = "mp3";
                audioType = AudioType.MPEG;
            }
            else
            {
                switch (cacheFormat)
                {
                    case CacheFormat.Wav:
                        extension = "wav";
                        audioType = AudioType.WAV;
                        break;
                    case CacheFormat.Ogg:
                        extension = "ogg";
                        audioType = AudioType.OGGVORBIS;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(cacheFormat), cacheFormat, null);
                }
            }

            await Rest.ValidateCacheDirectoryAsync();

            var downloadDirectory = Rest.DownloadCacheDirectory
                .CreateNewDirectory(nameof(ElevenLabs))
                .CreateNewDirectory(nameof(TextToSpeech))
                .CreateNewDirectory(voice.Id);

            CachedPath = $"{downloadDirectory}/{Id}.{extension}";

            if (!File.Exists(CachedPath))
            {
                switch (audioType)
                {
                    case AudioType.MPEG:
                        await File.WriteAllBytesAsync(CachedPath, ClipData.ToArray(), cancellationToken).ConfigureAwait(false);
                        break;
                    case AudioType.OGGVORBIS:
                        var oggBytes = await OggEncoder.ConvertToBytesAsync(
                            samples: ClipSamples.ToArray(),
                            sampleRate: outputFormat.GetSampleRate(),
                            channels: 1,
                            cancellationToken: cancellationToken).ConfigureAwait(false);
                        await File.WriteAllBytesAsync(CachedPath, oggBytes, cancellationToken).ConfigureAwait(false);
                        break;
                    case AudioType.WAV:
                        await WavEncoder.WriteToFileAsync(
                            path: CachedPath,
                            pcmData: ClipData.ToArray(),
                            sampleRate: outputFormat.GetSampleRate(),
                            channels: 1,
                            cancellationToken: cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
#endif // PLATFORM_WEBGL
        }
    }
}
