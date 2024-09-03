// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using UnityEngine.Scripting;

namespace ElevenLabs.Voices
{
    public sealed class SharedVoiceInfo
    {
        [Preserve]
        [JsonConstructor]
        internal SharedVoiceInfo(
            [JsonProperty("public_owner_id")] string ownerId,
            [JsonProperty("voice_id")] string voiceId,
            [JsonProperty("date_unix")] int dateUnix,
            [JsonProperty("name")] string name,
            [JsonProperty("accent")] string accent,
            [JsonProperty("gender")] string gender,
            [JsonProperty("age")] string age,
            [JsonProperty("descriptive")] string descriptive,
            [JsonProperty("use_case")] string useCase,
            [JsonProperty("category")] string category,
            [JsonProperty("language")] string language,
            [JsonProperty("description")] string description,
            [JsonProperty("preview_url")] string previewUrl,
            [JsonProperty("usage_character_count_1y")] int usageCharacterCount1Y,
            [JsonProperty("usage_character_count_7d")] int usageCharacterCount7D,
            [JsonProperty("play_api_usage_character_count_1y")] int playApiUsageCharacterCount1Y,
            [JsonProperty("cloned_by_count")] int clonedByCount,
            [JsonProperty("rate")] float rate,
            [JsonProperty("free_users_allowed")] bool freeUsersAllowed,
            [JsonProperty("live_moderation_enabled")] bool liveModerationEnabled,
            [JsonProperty("featured")] bool featured,
            [JsonProperty("notice_period")] int? noticePeriod,
            [JsonProperty("instagram_username")] string instagramUsername,
            [JsonProperty("twitter_username")] string twitterUsername,
            [JsonProperty("youtube_username")] string youtubeUsername,
            [JsonProperty("tiktok_username")] string tikTokUsername,
            [JsonProperty("image_url")] string imageUrl)
        {
            OwnerId = ownerId;
            VoiceId = voiceId;
            DateUnix = dateUnix;
            Name = name;
            Accent = accent;
            Gender = gender;
            Age = age;
            Descriptive = descriptive;
            UseCase = useCase;
            Category = category;
            Language = language;
            Description = description;
            PreviewUrl = previewUrl;
            UsageCharacterCount1Y = usageCharacterCount1Y;
            UsageCharacterCount7D = usageCharacterCount7D;
            PlayApiUsageCharacterCount1Y = playApiUsageCharacterCount1Y;
            ClonedByCount = clonedByCount;
            Rate = rate;
            FreeUsersAllowed = freeUsersAllowed;
            LiveModerationEnabled = liveModerationEnabled;
            Featured = featured;
            NoticePeriod = noticePeriod;
            InstagramUsername = instagramUsername;
            TwitterUsername = twitterUsername;
            YoutubeUsername = youtubeUsername;
            TikTokUsername = tikTokUsername;
            ImageUrl = imageUrl;
        }

        [Preserve]
        [JsonProperty("public_owner_id")]
        public string OwnerId { get; }

        [Preserve]
        [JsonProperty("voice_id")]
        public string VoiceId { get; }

        [Preserve]
        [JsonProperty("date_unix")]
        public int DateUnix { get; }

        [JsonIgnore]
        public DateTime Date => DateTimeOffset.FromUnixTimeSeconds(DateUnix).DateTime;

        [Preserve]
        [JsonProperty("name")]
        public string Name { get; }

        [Preserve]
        [JsonProperty("accent")]
        public string Accent { get; }

        [Preserve]
        [JsonProperty("gender")]
        public string Gender { get; }

        [Preserve]
        [JsonProperty("age")]
        public string Age { get; }

        [Preserve]
        [JsonProperty("descriptive")]
        public string Descriptive { get; }

        [Preserve]
        [JsonProperty("use_case")]
        public string UseCase { get; }

        [Preserve]
        [JsonProperty("category")]
        public string Category { get; }

        [Preserve]
        [JsonProperty("language")]
        public string Language { get; }

        [Preserve]
        [JsonProperty("description")]
        public string Description { get; }

        [Preserve]
        [JsonProperty("preview_url")]
        public string PreviewUrl { get; }

        [Preserve]
        [JsonProperty("usage_character_count_1y")]
        public int UsageCharacterCount1Y { get; }

        [Preserve]
        [JsonProperty("usage_character_count_7d")]
        public int UsageCharacterCount7D { get; }

        [Preserve]
        [JsonProperty("play_api_usage_character_count_1y")]
        public int PlayApiUsageCharacterCount1Y { get; }

        [Preserve]
        [JsonProperty("cloned_by_count")]
        public int ClonedByCount { get; }

        [Preserve]
        [JsonProperty("rate")]
        public float Rate { get; }

        [Preserve]
        [JsonProperty("free_users_allowed")]
        public bool FreeUsersAllowed { get; }

        [Preserve]
        [JsonProperty("live_moderation_enabled")]
        public bool LiveModerationEnabled { get; }

        [Preserve]
        [JsonProperty("featured")]
        public bool Featured { get; }

        [Preserve]
        [JsonProperty("notice_period")]
        public int? NoticePeriod { get; }

        [Preserve]
        [JsonProperty("instagram_username")]
        public string InstagramUsername { get; }

        [Preserve]
        [JsonProperty("twitter_username")]
        public string TwitterUsername { get; }

        [Preserve]
        [JsonProperty("youtube_username")]
        public string YoutubeUsername { get; }

        [Preserve]
        [JsonProperty("tiktok_username")]
        public string TikTokUsername { get; }

        [Preserve]
        [JsonProperty("image_url")]
        public string ImageUrl { get; }
    }
}
