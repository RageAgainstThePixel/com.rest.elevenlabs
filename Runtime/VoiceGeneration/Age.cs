// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace ElevenLabs
{
    public sealed class Age
    {
        [JsonConstructor]
        public Age(
            [JsonProperty("name")] string name,
            [JsonProperty("code")] string code
        )
        {
            Name = name;
            Code = code;
        }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("code")]
        public string Code { get; }
    }
}
