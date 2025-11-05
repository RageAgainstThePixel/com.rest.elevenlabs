// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ElevenLabs
{
    [Preserve]
    public sealed class PronunciationDictionaryLocator
    {
        [Preserve]
        [JsonConstructor]
        public PronunciationDictionaryLocator(
            [JsonProperty("pronunciation_dictionary_id")] string id,
            [JsonProperty("version_id")] string version)
        {
            Id = id;
            Version = version;
        }

        [Preserve]
        [JsonProperty("pronunciation_dictionary_id")]
        public string Id { get; }


        [Preserve]
        [JsonProperty("version_id")]
        public string Version { get; }

        [Preserve]
        public static implicit operator PronunciationDictionaryLocator(PronunciationDictionary dict)
            => new(dict.Id, dict.LatestVersionId);
    }
}
