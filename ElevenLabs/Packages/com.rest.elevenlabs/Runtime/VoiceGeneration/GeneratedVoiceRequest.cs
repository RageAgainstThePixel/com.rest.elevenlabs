// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace ElevenLabs.VoiceGeneration
{
    public sealed class GeneratedVoiceRequest
    {
        [JsonConstructor]
        public GeneratedVoiceRequest(
            [JsonProperty("text")] string text,
            [JsonProperty("gender")] string gender,
            [JsonProperty("accent")] string accent,
            [JsonProperty("age")] string age,
            [JsonProperty("accent_strength")] int accentStrength
        )
        {
            Text = text;
            Gender = gender;
            Accent = accent;
            Age = age;
            AccentStrength = accentStrength;
        }

        [JsonProperty("text")]
        public string Text { get; }

        [JsonProperty("gender")]
        public string Gender { get; }

        [JsonProperty("accent")]
        public string Accent { get; }

        [JsonProperty("age")]
        public string Age { get; }

        [JsonProperty("accent_strength")]
        public int AccentStrength { get; }
    }
}
