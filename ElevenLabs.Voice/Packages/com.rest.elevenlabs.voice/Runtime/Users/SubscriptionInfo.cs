// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ElevenLabs.User
{
    public sealed class SubscriptionInfo
    {
        [JsonConstructor]
        public SubscriptionInfo(
            [JsonProperty("tier")] string tier,
            [JsonProperty("character_count")] int characterCount,
            [JsonProperty("character_limit")] int characterLimit,
            [JsonProperty("can_extend_character_limit")] bool canExtendCharacterLimit,
            [JsonProperty("allowed_to_extend_character_limit")] bool allowedToExtendCharacterLimit,
            [JsonProperty("next_character_count_reset_unix")] int nextCharacterCountResetUnix,
            [JsonProperty("voice_limit")] int voiceLimit,
            [JsonProperty("can_extend_voice_limit")] bool canExtendVoiceLimit,
            [JsonProperty("can_use_instant_voice_cloning")] bool canUseInstantVoiceCloning,
            [JsonProperty("available_models")] List<AvailableModel> availableModels,
            [JsonProperty("status")] string status,
            [JsonProperty("next_invoice")] NextInvoice nextInvoice)
        {
            Tier = tier;
            CharacterCount = characterCount;
            CharacterLimit = characterLimit;
            CanExtendCharacterLimit = canExtendCharacterLimit;
            AllowedToExtendCharacterLimit = allowedToExtendCharacterLimit;
            NextCharacterCountResetUnix = nextCharacterCountResetUnix;
            VoiceLimit = voiceLimit;
            CanExtendVoiceLimit = canExtendVoiceLimit;
            CanUseInstantVoiceCloning = canUseInstantVoiceCloning;
            AvailableModels = availableModels;
            Status = status;
            NextInvoice = nextInvoice;
        }

        [JsonProperty("tier")]
        public string Tier { get; }

        [JsonProperty("character_count")]
        public int CharacterCount { get; }

        [JsonProperty("character_limit")]
        public int CharacterLimit { get; }

        [JsonProperty("can_extend_character_limit")]
        public bool CanExtendCharacterLimit { get; }

        [JsonProperty("allowed_to_extend_character_limit")]
        public bool AllowedToExtendCharacterLimit { get; }

        [JsonProperty("next_character_count_reset_unix")]
        public int NextCharacterCountResetUnix { get; }

        [JsonIgnore]
        public DateTime NextCharacterCountReset => DateTimeOffset.FromUnixTimeSeconds(NextCharacterCountResetUnix).DateTime;

        [JsonProperty("voice_limit")]
        public int VoiceLimit { get; }

        [JsonProperty("can_extend_voice_limit")]
        public bool CanExtendVoiceLimit { get; }

        [JsonProperty("can_use_instant_voice_cloning")]
        public bool CanUseInstantVoiceCloning { get; }

        [JsonProperty("available_models")]
        public IReadOnlyList<AvailableModel> AvailableModels { get; }

        [JsonProperty("status")]
        public string Status { get; }

        [JsonProperty("next_invoice")]
        public NextInvoice NextInvoice { get; }
    }
}
