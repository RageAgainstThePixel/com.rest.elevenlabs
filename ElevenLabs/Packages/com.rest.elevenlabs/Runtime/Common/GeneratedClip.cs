// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Utilities.Audio;
using Utilities.WebRequestRest;

namespace ElevenLabs
{
    [Preserve]
    [Serializable]
    public class GeneratedClip : ISerializationCallbackReceiver
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
        }

        [Preserve]
        internal GeneratedClip(string id, string text, ReadOnlyMemory<byte> clipData, int sampleRate, string cachedPath = null)
        {
            this.id = id;
            this.text = text;
            TextHash = $"{id}{text}".GenerateGuid();
            textHash = TextHash.ToString();
            this.cachedPath = cachedPath;
            ClipData = clipData;
            SampleRate = sampleRate;
        }

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
        private AudioClip audioClip;

        [Preserve]
        public AudioClip AudioClip
        {
            get
            {
                if (audioClip == null && !ClipData.IsEmpty)
                {
                    var samples = ClipSamples;

                    if (samples is { Length: > 0 })
                    {
                        audioClip = AudioClip.Create(Id, samples.Length, 1, SampleRate, false);
                        audioClip.SetData(samples, 0);
                    }
                }

                if (audioClip == null)
                {
                    Debug.LogError($"{nameof(audioClip)} is null, try loading it with LoadCachedAudioClipAsync");
                }

                return audioClip;
            }
        }

        [SerializeField]
        private string cachedPath;

        [Preserve]
        public string CachedPath => cachedPath;

        public ReadOnlyMemory<byte> ClipData { get; }

        private float[] clipSamples;

        public float[] ClipSamples
        {
            get
            {
                if (!ClipData.IsEmpty)
                {
                    clipSamples ??= PCMEncoder.Decode(ClipData.ToArray(), PCMFormatSize.SixteenBit, SampleRate, AudioSettings.outputSampleRate);
                }
                else if (audioClip != null)
                {
                    clipSamples = new float[audioClip.samples];
                    audioClip.GetData(clipSamples, 0);
                }

                return clipSamples;
            }
        }

        public int SampleRate { get; }

        public void OnBeforeSerialize() => textHash = TextHash.ToString();

        public void OnAfterDeserialize() => TextHash = Guid.Parse(textHash);

        public static implicit operator AudioClip(GeneratedClip clip) => clip?.AudioClip;

        public async Task<AudioClip> LoadCachedAudioClipAsync(CancellationToken cancellationToken = default)
        {
            var audioType = cachedPath switch
            {
                var path when path.EndsWith(".ogg") => AudioType.OGGVORBIS,
                var path when path.EndsWith(".wav") => AudioType.WAV,
                var path when path.EndsWith(".mp3") => AudioType.MPEG,
                _ => AudioType.UNKNOWN
            };

            return await Rest.DownloadAudioClipAsync($"file://{cachedPath}", audioType, cancellationToken: cancellationToken);
        }
    }
}
