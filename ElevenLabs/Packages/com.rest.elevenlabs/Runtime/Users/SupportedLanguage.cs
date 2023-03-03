// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace ElevenLabs.User
{
    public sealed class SupportedLanguage
    {
        [JsonConstructor]
        public SupportedLanguage(
            [JsonProperty("iso_code")] string isoCode,
            [JsonProperty("display_name")] string displayName)
        {
            IsoCode = isoCode;
            DisplayName = displayName;
        }

        [JsonProperty("iso_code")]
        public string IsoCode { get; }

        [JsonProperty("display_name")]
        public string DisplayName { get; }
    }
}
