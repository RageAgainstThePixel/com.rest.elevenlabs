// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace ElevenLabs.Voices
{
    public sealed class VoiceSettings
    {
        [JsonConstructor]
        public VoiceSettings(
            [JsonProperty("stability")] int stability,
            [JsonProperty("similarity_boost")] int similarityBoost)
        {
            Stability = stability;
            SimilarityBoost = similarityBoost;
        }

        [JsonProperty("stability")]
        public int Stability { get; }

        [JsonProperty("similarity_boost")]
        public int SimilarityBoost { get; }
    }
}
