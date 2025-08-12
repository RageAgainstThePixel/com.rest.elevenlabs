// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace ElevenLabs.Voices
{
    /// <summary>
    /// Represents a container for a collection of voices with pagination metadata.
    /// </summary>
    [Preserve]
    public sealed class VoiceList
    {
        [Preserve]
        [JsonConstructor]
        internal VoiceList(
            [JsonProperty("voices")] List<Voice> voices,
            [JsonProperty("has_more")] bool hasMore,
            [JsonProperty("total_count")] int totalCount,
            [JsonProperty("next_page_token")] string nextPageToken)
        {
            Voices = voices;
            HasMore = hasMore;
            TotalCount = totalCount;
            NextPageToken = nextPageToken;
        }

        [Preserve]
        [JsonProperty("voices")]
        public IReadOnlyList<Voice> Voices { get; }

        [Preserve]
        [JsonProperty("has_more")]
        public bool HasMore { get; }

        [Preserve]
        [JsonProperty("total_count")]
        public int TotalCount { get; }

        [Preserve]
        [JsonProperty("next_page_token")]
        public string NextPageToken { get; }
    }
}
