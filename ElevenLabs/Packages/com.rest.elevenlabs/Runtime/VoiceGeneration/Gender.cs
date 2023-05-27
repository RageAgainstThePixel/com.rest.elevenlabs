// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ElevenLabs.VoiceGeneration
{
    [Preserve]
    public sealed class Gender
    {
        [Preserve]
        [JsonConstructor]
        public Gender(
            [JsonProperty("name")] string name,
            [JsonProperty("code")] string code)
        {
            Name = name;
            Code = code;
        }

        [Preserve]
        [JsonProperty("name")]
        public string Name { get; }

        [Preserve]
        [JsonProperty("code")]
        public string Code { get; }
    }
}
