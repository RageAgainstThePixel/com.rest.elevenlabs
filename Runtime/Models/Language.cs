// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace ElevenLabs.Models
{
    public sealed class Language
    {
        [JsonConstructor]
        public Language(
            [JsonProperty("language_id")] string id,
            [JsonProperty("name")] string name)
        {
            Id = id;
            Name = name;
        }

        [JsonProperty("language_id")]
        public string Id { get; }

        [JsonProperty("name")]
        public string Name { get; }
    }
}
