// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace ElevenLabs.User
{
    /// <summary>
    /// Access to your user account information.
    /// </summary>
    public sealed class UserEndpoint : BaseEndPoint
    {
        public UserEndpoint(ElevenLabsClient api) : base(api) { }

        protected override string GetEndpoint()
            => $"{Api.BaseUrl}user";

        /// <summary>
        /// Gets information about your user account.
        /// </summary>
        public async Task<UserInfo> GetUserInfoAsync()
        {
            var response = await Api.Client.GetAsync($"{GetEndpoint()}");
            var responseAsString = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UserInfo>(responseAsString, Api.JsonSerializationOptions);
        }

        /// <summary>
        /// Gets your subscription info.
        /// </summary>
        public async Task<SubscriptionInfo> GetSubscriptionInfoAsync()
        {
            var response = await Api.Client.GetAsync($"{GetEndpoint()}/subscription");
            var responseAsString = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SubscriptionInfo>(responseAsString, Api.JsonSerializationOptions);
        }
    }
}
