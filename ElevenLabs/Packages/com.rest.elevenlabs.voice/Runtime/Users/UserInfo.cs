// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace ElevenLabs.User
{
    public sealed class UserInfo
    {
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

        [JsonProperty("subscription")]
        public SubscriptionInfo SubscriptionInfo { get; }

        [JsonProperty("is_new_user")]
        public bool IsNewUser { get; }

        [JsonProperty("xi_api_key")]
        public string XiApiKey { get; }
    }
}
