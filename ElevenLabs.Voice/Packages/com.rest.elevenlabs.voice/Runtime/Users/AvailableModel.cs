// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace ElevenLabs.User
{
    public sealed class AvailableModel
    {
        [JsonConstructor]
        public AvailableModel(
            [JsonProperty("model_id")] string modelId,
            [JsonProperty("display_name")] string displayName,
            [JsonProperty("supported_languages")] List<SupportedLanguage> supportedLanguages        )
        {
            ModelId = modelId;
            DisplayName = displayName;
            SupportedLanguages = supportedLanguages;
        }

        [JsonProperty("model_id")]
        public string ModelId { get; }

        [JsonProperty("display_name")]
        public string DisplayName { get; }

        [JsonProperty("supported_languages")]
        public IReadOnlyList<SupportedLanguage> SupportedLanguages { get; }
    }
}
