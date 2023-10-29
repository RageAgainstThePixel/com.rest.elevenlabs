// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace ElevenLabs.History
{
    [Preserve]
    public sealed class HistoryInfo
    {
        [Preserve]
        [JsonConstructor]
        public HistoryInfo(
            [JsonProperty("history")] List<HistoryItem> historyItems,
            [JsonProperty("last_history_item_id")] string lastHistoryItemId,
            [JsonProperty("has_more")] bool hasMore)
        {
            HistoryItems = historyItems;
            LastHistoryItemId = lastHistoryItemId;
            HasMore = hasMore;
        }

        [Preserve]
        [JsonProperty("history")]
        public IReadOnlyList<HistoryItem> HistoryItems { get; }

        [Preserve]
        [JsonProperty("last_history_item_id")]
        public string LastHistoryItemId { get; }

        [Preserve]
        [JsonProperty("has_more")]
        public bool HasMore { get; }
    }
}
