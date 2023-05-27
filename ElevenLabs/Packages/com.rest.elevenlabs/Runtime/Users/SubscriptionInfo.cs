// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace ElevenLabs.User
{
    [Preserve]
    public sealed class SubscriptionInfo
    {
        [Preserve]
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

        [Preserve]
        [JsonProperty("tier")]
        public string Tier { get; }

        [Preserve]
        [JsonProperty("character_count")]
        public int CharacterCount { get; }

        [Preserve]
        [JsonProperty("character_limit")]
        public int CharacterLimit { get; }

        [Preserve]
        [JsonProperty("can_extend_character_limit")]
        public bool CanExtendCharacterLimit { get; }

        [Preserve]
        [JsonProperty("allowed_to_extend_character_limit")]
        public bool AllowedToExtendCharacterLimit { get; }

        [Preserve]
        [JsonProperty("next_character_count_reset_unix")]
        public int NextCharacterCountResetUnix { get; }

        [Preserve]
        [JsonIgnore]
        public DateTime NextCharacterCountReset => DateTimeOffset.FromUnixTimeSeconds(NextCharacterCountResetUnix).DateTime;

        [Preserve]
        [JsonProperty("voice_limit")]
        public int VoiceLimit { get; }

        [Preserve]
        [JsonProperty("can_extend_voice_limit")]
        public bool CanExtendVoiceLimit { get; }

        [Preserve]
        [JsonProperty("can_use_instant_voice_cloning")]
        public bool CanUseInstantVoiceCloning { get; }

        [Preserve]
        [JsonProperty("available_models")]
        public IReadOnlyList<AvailableModel> AvailableModels { get; }

        [Preserve]
        [JsonProperty("status")]
        public string Status { get; }

        [Preserve]
        [JsonProperty("next_invoice")]
        public NextInvoice NextInvoice { get; }
    }
}
