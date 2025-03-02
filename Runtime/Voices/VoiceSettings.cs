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
        [Obsolete("use new .ctr overload")]
        public VoiceSettings(
            float stability,
            float similarityBoost,
            bool speakerBoost,
            float style)
            : this(stability, similarityBoost, style, speakerBoost)
        {
        }

        [Preserve]
        [JsonConstructor]
        public VoiceSettings(
            [JsonProperty("stability")] float stability = 0.75f,
            [JsonProperty("similarity_boost")] float similarityBoost = 0.75f,
            [JsonProperty("style")] float style = 0.45f,
            [JsonProperty("use_speaker_boost")] bool speakerBoost = true,
            [JsonProperty("speed")] float speed = 1f)
        {
            Stability = stability;
            SimilarityBoost = similarityBoost;
            Style = style;
            SpeakerBoost = speakerBoost;
            Speed = speed;
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

        [SerializeField]
        private float speed = 1f;

        [Preserve]
        [JsonProperty("speed", DefaultValueHandling = DefaultValueHandling.Include)]
        public float Speed
        {
            get => speed;
            set => speed = value;
        }
    }
}
