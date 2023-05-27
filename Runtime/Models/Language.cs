// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ElevenLabs.Models
{
    [Preserve]
    public sealed class Language
    {
        [Preserve]
        [JsonConstructor]
        public Language(
            [JsonProperty("language_id")] string id,
            [JsonProperty("name")] string name)
        {
            Id = id;
            Name = name;
        }

        [Preserve]
        [JsonProperty("language_id")]
        public string Id { get; }

        [Preserve]
        [JsonProperty("name")]
        public string Name { get; }
    }
}
