// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using Utilities.Audio;
using Utilities.WebRequestRest;

namespace ElevenLabs
{
    [Preserve]
    [Serializable]
    public class GeneratedClip : ISerializationCallbackReceiver, IDisposable
    {
        [Preserve]
        internal GeneratedClip(string id, string text, AudioClip audioClip, string cachedPath = null)
        {
            this.id = id;
            this.text = text;
            TextHash = $"{id}{text}".GenerateGuid();
            textHash = TextHash.ToString();
            this.cachedPath = cachedPath;
            SampleRate = audioClip.frequency;
            this.audioClip = audioClip;
        }

        [Preserve]
        internal GeneratedClip(string id, string text, NativeArray<byte> clipData, int sampleRate, string cachedPath = null)
        {
            this.id = id;
            this.text = text;
            TextHash = $"{id}{text}".GenerateGuid();
            textHash = TextHash.ToString();
            this.cachedPath = cachedPath;
            this.clipData = clipData;
            SampleRate = sampleRate;
        }

        ~GeneratedClip()
            => Dispose();

        [SerializeField]
        private string id;

        [Preserve]
        public string Id => id;

        [SerializeField]
        private string text;

        [Preserve]
        public string Text => text;

        [SerializeField]
        private string textHash;

        [Preserve]
        public Guid TextHash { get; private set; }

        [SerializeField]
        private string cachedPath;

        [Preserve]
        public string CachedPath => cachedPath;

        [Preserve]
        public NativeArray<byte> ClipData => clipData ??= new NativeArray<byte>(0, Allocator.Persistent);
        private NativeArray<byte>? clipData;

        [Preserve]
        public NativeArray<float> ClipSamples
        {
            get
            {
                if (clipSamples != null)
                {
                    return clipSamples.Value;
                }

                if (ClipData.Length == 0)
                {
                    return new NativeArray<float>(0, Allocator.Persistent);
                }

                clipSamples ??= PCMEncoder.Decode(pcmData: ClipData, inputSampleRate: SampleRate, outputSampleRate: AudioSettings.outputSampleRate, allocator: Allocator.Persistent);
                return clipSamples.Value;

            }
        }
        private NativeArray<float>? clipSamples;

        public int SampleRate { get; }

        [SerializeField]
        private AudioClip audioClip;

        [Preserve]
        public AudioClip AudioClip
        {
            get
            {
                if (audioClip == null &&
                    ClipSamples is { Length: > 0 })
                {
                    audioClip = AudioClip.Create(Id, ClipSamples.Length, 1, AudioSettings.outputSampleRate, false);
#if UNITY_6000_0_OR_NEWER
                    audioClip.SetData(ClipSamples, 0);
#else
                    audioClip.SetData(ClipSamples.ToArray(), 0);
#endif
                }

                if (audioClip == null)
                {
                    Debug.LogError($"{nameof(audioClip)} is null, try loading it with {nameof(LoadCachedAudioClipAsync)}");
                }

                return audioClip;
            }
        }

        public float Length => ClipSamples.Length / (float)AudioSettings.outputSampleRate;

        public void OnBeforeSerialize() => textHash = TextHash.ToString();

        public void OnAfterDeserialize() => TextHash = Guid.Parse(textHash);

        public static implicit operator AudioClip(GeneratedClip clip) => clip?.AudioClip;

        public Task<AudioClip> LoadCachedAudioClipAsync(bool debug = false, CancellationToken cancellationToken = default)
        {
            var audioType = cachedPath switch
            {
                var path when path.EndsWith(".ogg") => AudioType.OGGVORBIS,
                var path when path.EndsWith(".wav") => AudioType.WAV,
                var path when path.EndsWith(".mp3") => AudioType.MPEG,
                _ => AudioType.UNKNOWN
            };

            if (audioType == AudioType.UNKNOWN)
            {
                Debug.LogWarning($"Unable to load cached audio clip at {cachedPath}");
                return null;
            }

            return Rest.DownloadAudioClipAsync(
                url: $"file://{cachedPath}",
                audioType: audioType,
                parameters: new RestParameters(debug: debug),
                cancellationToken: cancellationToken);
        }

        public void Dispose()
        {
            clipData?.Dispose();
            clipSamples?.Dispose();
        }
    }
}
