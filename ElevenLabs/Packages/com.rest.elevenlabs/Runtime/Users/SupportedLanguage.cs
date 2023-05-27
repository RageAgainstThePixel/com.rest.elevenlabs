// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ElevenLabs.User
{
    [Preserve]
    public sealed class SupportedLanguage
    {
        [Preserve]
        [JsonConstructor]
        public SupportedLanguage(
            [JsonProperty("iso_code")] string isoCode,
            [JsonProperty("display_name")] string displayName)
        {
            IsoCode = isoCode;
            DisplayName = displayName;
        }

        [Preserve]
        [JsonProperty("iso_code")]
        public string IsoCode { get; }

        [Preserve]
        [JsonProperty("display_name")]
        public string DisplayName { get; }
    }
}
