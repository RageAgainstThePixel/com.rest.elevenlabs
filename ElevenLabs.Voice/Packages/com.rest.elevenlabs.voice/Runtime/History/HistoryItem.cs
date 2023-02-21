// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;

namespace ElevenLabs.History
{
    public sealed class HistoryItem
    {
        [JsonConstructor]
        public HistoryItem(
            [JsonProperty("history_item_id")] string historyItemId,
            [JsonProperty("voice_id")] string voiceId,
            [JsonProperty("voice_name")] string voiceName,
            [JsonProperty("text")] string text,
            [JsonProperty("date_unix")] int dateUnix,
            [JsonProperty("character_count_change_from")] int characterCountChangeFrom,
            [JsonProperty("character_count_change_to")] int characterCountChangeTo,
            [JsonProperty("content_type")] string contentType,
            [JsonProperty("state")] string state)
        {
            HistoryItemId = historyItemId;
            VoiceId = voiceId;
            VoiceName = voiceName;
            Text = text;
            DateUnix = dateUnix;
            CharacterCountChangeFrom = characterCountChangeFrom;
            CharacterCountChangeTo = characterCountChangeTo;
            ContentType = contentType;
            State = state;
        }

        [JsonProperty("history_item_id")]
        public string HistoryItemId { get; }

        [JsonProperty("voice_id")]
        public string VoiceId { get; }

        [JsonProperty("voice_name")]
        public string VoiceName { get; }

        [JsonProperty("text")]
        public string Text { get; }

        [JsonProperty("date_unix")]
        public int DateUnix { get; }

        [JsonIgnore]
        public DateTime Date => DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime;

        [JsonProperty("character_count_change_from")]
        public int CharacterCountChangeFrom { get; }

        [JsonProperty("character_count_change_to")]
        public int CharacterCountChangeTo { get; }

        [JsonProperty("content_type")]
        public string ContentType { get; }

        [JsonProperty("state")]
        public string State { get; }
    }
}
