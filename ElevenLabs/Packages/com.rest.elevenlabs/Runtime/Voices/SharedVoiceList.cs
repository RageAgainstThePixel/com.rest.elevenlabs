// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace ElevenLabs.Voices
{
    [Preserve]
    public sealed class SharedVoiceList
    {
        [Preserve]
        [JsonConstructor]
        internal SharedVoiceList(
            [JsonProperty("voices")] IReadOnlyList<SharedVoiceInfo> voices,
            [JsonProperty("has_more")] bool hasMore,
            [JsonProperty("last_sort_id")] string lastId)
        {
            Voices = voices;
            HasMore = hasMore;
            LastId = lastId;
        }

        [Preserve]
        [JsonProperty("voices")]
        public IReadOnlyList<SharedVoiceInfo> Voices { get; }

        [Preserve]
        [JsonProperty("has_more")]
        public bool HasMore { get; }

        [Preserve]
        [JsonProperty("last_sort_id")]
        public string LastId { get; }
    }
}
