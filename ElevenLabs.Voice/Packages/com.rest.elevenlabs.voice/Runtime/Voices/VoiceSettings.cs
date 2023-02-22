// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace ElevenLabs.Voices
{
    public sealed class VoiceSettings
    {
        [JsonConstructor]
        public VoiceSettings(
            [JsonProperty("stability")] double stability,
            [JsonProperty("similarity_boost")] double similarityBoost)
        {
            Stability = stability;
            SimilarityBoost = similarityBoost;
        }

        [JsonProperty("stability")]
        public double Stability { get; }

        [JsonProperty("similarity_boost")]
        public double SimilarityBoost { get; }
    }
}
