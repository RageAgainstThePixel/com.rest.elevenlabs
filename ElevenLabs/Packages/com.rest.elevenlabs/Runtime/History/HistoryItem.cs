// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using Newtonsoft.Json;
using System;
using UnityEngine.Scripting;

namespace ElevenLabs.History
{
    [Preserve]
    public sealed class HistoryItem
    {
        [Preserve]
        public static implicit operator string(HistoryItem historyItem) => historyItem.Id;

        [Preserve]
        [JsonConstructor]
        public HistoryItem(
            [JsonProperty("history_item_id")] string id,
            [JsonProperty("voice_id")] string voiceId,
            [JsonProperty("voice_name")] string voiceName,
            [JsonProperty("text")] string text,
            [JsonProperty("date_unix")] int dateUnix,
            [JsonProperty("character_count_change_from")] int characterCountChangeFrom,
            [JsonProperty("character_count_change_to")] int characterCountChangeTo,
            [JsonProperty("content_type")] string contentType,
            [JsonProperty("state")] string state)
        {
            Id = id;
            VoiceId = voiceId;
            VoiceName = voiceName;
            Text = text;
            TextHash = $"{id}{text}".GenerateGuid().ToString();
            DateUnix = dateUnix;
            CharacterCountChangeFrom = characterCountChangeFrom;
            CharacterCountChangeTo = characterCountChangeTo;
            ContentType = contentType;
            State = state;
        }

        [Preserve]
        [JsonProperty("history_item_id")]
        public string Id { get; }

        [Preserve]
        [JsonProperty("voice_id")]
        public string VoiceId { get; }

        [Preserve]
        [JsonProperty("voice_name")]
        public string VoiceName { get; }

        [Preserve]
        [JsonProperty("text")]
        public string Text { get; }

        [Preserve]
        [JsonIgnore]
        public string TextHash { get; }

        [Preserve]
        [JsonProperty("date_unix")]
        public int DateUnix { get; }

        [Preserve]
        [JsonIgnore]
        public DateTime Date => DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime;

        [Preserve]
        [JsonProperty("character_count_change_from")]
        public int CharacterCountChangeFrom { get; }

        [Preserve]
        [JsonProperty("character_count_change_to")]
        public int CharacterCountChangeTo { get; }

        [Preserve]
        [JsonProperty("content_type")]
        public string ContentType { get; }

        [Preserve]
        [JsonProperty("state")]
        public string State { get; }
    }
}
