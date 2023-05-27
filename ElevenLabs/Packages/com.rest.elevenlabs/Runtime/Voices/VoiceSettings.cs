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
            [JsonProperty("stability")] float stability,
            [JsonProperty("similarity_boost")] float similarityBoost)
        {
            Stability = stability;
            SimilarityBoost = similarityBoost;
        }

        [Range(0f, 1f)]
        [SerializeField]
        private float stability = 0.75f;

        [Preserve]
        [JsonProperty("stability")]
        public float Stability
        {
            get => stability;
            private set => stability = value;
        }

        [Range(0f, 1f)]
        [SerializeField]
        private float similarityBoost = 0.75f;

        [Preserve]
        [JsonProperty("similarity_boost")]
        public float SimilarityBoost
        {
            get => similarityBoost;
            private set => similarityBoost = value;
        }
    }
}
