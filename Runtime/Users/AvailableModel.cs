// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace ElevenLabs.User
{
    [Preserve]
    public sealed class AvailableModel
    {
        [Preserve]
        [JsonConstructor]
        public AvailableModel(
            [JsonProperty("model_id")] string modelId,
            [JsonProperty("display_name")] string displayName,
            [JsonProperty("supported_languages")] List<SupportedLanguage> supportedLanguages)
        {
            ModelId = modelId;
            DisplayName = displayName;
            SupportedLanguages = supportedLanguages;
        }

        [Preserve]
        [JsonProperty("model_id")]
        public string ModelId { get; }

        [Preserve]
        [JsonProperty("display_name")]
        public string DisplayName { get; }

        [Preserve]
        [JsonProperty("supported_languages")]
        public IReadOnlyList<SupportedLanguage> SupportedLanguages { get; }
    }
}
