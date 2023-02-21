// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace ElevenLabs.Voices
{
    public sealed class Labels
    {
        [JsonConstructor]
        public Labels(
            [JsonProperty("additionalProp1")] string additionalProp1,
            [JsonProperty("additionalProp2")] string additionalProp2,
            [JsonProperty("additionalProp3")] string additionalProp3)
        {
            AdditionalProp1 = additionalProp1;
            AdditionalProp2 = additionalProp2;
            AdditionalProp3 = additionalProp3;
        }

        [JsonProperty("additionalProp1")]
        public string AdditionalProp1 { get; }

        [JsonProperty("additionalProp2")]
        public string AdditionalProp2 { get; }

        [JsonProperty("additionalProp3")]
        public string AdditionalProp3 { get; }
    }
}
