// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ElevenLabs.User
{
    [Preserve]
    public sealed class UserInfo
    {
        [Preserve]
        [JsonConstructor]
        public UserInfo(
            [JsonProperty("subscription")] SubscriptionInfo subscriptionInfo,
            [JsonProperty("is_new_user")] bool isNewUser,
            [JsonProperty("xi_api_key")] string xiApiKey)
        {
            SubscriptionInfo = subscriptionInfo;
            IsNewUser = isNewUser;
            XiApiKey = xiApiKey;
        }

        [Preserve]
        [JsonProperty("subscription")]
        public SubscriptionInfo SubscriptionInfo { get; }

        [Preserve]
        [JsonProperty("is_new_user")]
        public bool IsNewUser { get; }

        [Preserve]
        [JsonProperty("xi_api_key")]
        public string XiApiKey { get; }
    }
}
