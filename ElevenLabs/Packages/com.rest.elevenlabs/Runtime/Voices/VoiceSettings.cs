// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace ElevenLabs.Voices
{
    [Preserve]
    [Serializable]
    public sealed class VoiceSettings
    {
        [Preserve]
        [JsonConstructor]
        public VoiceSettings(
            [JsonProperty("stability")] float stability = .75f,
            [JsonProperty("similarity_boost")] float similarityBoost = .75f,
            [JsonProperty("speaker_boost")] bool speakerBoost = true,
            [JsonProperty("style")] float style = 0.45f)
        {
            Stability = stability;
            SimilarityBoost = similarityBoost;
            SpeakerBoost = speakerBoost;
            Style = style;
        }

        [Range(0f, 1f)]
        [SerializeField]
        private float stability = 0.75f;

        [Preserve]
        [JsonProperty("stability", DefaultValueHandling = DefaultValueHandling.Include)]
        public float Stability
        {
            get => stability;
            set => stability = value;
        }

        [Range(0f, 1f)]
        [SerializeField]
        private float similarityBoost = 0.75f;

        [Preserve]
        [JsonProperty("similarity_boost", DefaultValueHandling = DefaultValueHandling.Include)]
        public float SimilarityBoost
        {
            get => similarityBoost;
            set => similarityBoost = value;
        }

        [Range(0f, 1f)]
        [SerializeField]
        private float style = 0.45f;

        [Preserve]
        [JsonProperty("style", DefaultValueHandling = DefaultValueHandling.Include)]
        public float Style
        {
            get => style;
            set => style = value;
        }

        [SerializeField]
        private bool speakerBoost = true;

        [Preserve]
        [JsonProperty("use_speaker_boost", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool SpeakerBoost
        {
            get => speakerBoost;
            set => speakerBoost = value;
        }
    }
}
