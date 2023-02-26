// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace ElevenLabs.Voices
{
    public sealed class VoiceSettings
    {
        [JsonConstructor]
        public VoiceSettings(
            [JsonProperty("stability")] float stability,
            [JsonProperty("similarity_boost")] float similarityBoost)
        {
            Stability = stability;
            SimilarityBoost = similarityBoost;
        }

        [JsonProperty("stability")]
        public float Stability { get; }

        [JsonProperty("similarity_boost")]
        public float SimilarityBoost { get; }
    }
}
