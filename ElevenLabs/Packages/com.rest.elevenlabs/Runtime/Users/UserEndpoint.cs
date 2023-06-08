// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Threading.Tasks;
using Utilities.WebRequestRest;

namespace ElevenLabs.User
{
    /// <summary>
    /// Access to your user account information.
    /// </summary>
    public sealed class UserEndpoint : ElevenLabsBaseEndPoint
    {
        public UserEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "user";

        /// <summary>
        /// Gets information about your user account.
        /// </summary>
        public async Task<UserInfo> GetUserInfoAsync()
        {
            var response = await Rest.GetAsync(GetUrl(), new RestParameters(client.DefaultRequestHeaders));
            response.ValidateResponse();
            return JsonConvert.DeserializeObject<UserInfo>(response.ResponseBody, client.JsonSerializationOptions);
        }

        /// <summary>
        /// Gets your subscription info.
        /// </summary>
        public async Task<SubscriptionInfo> GetSubscriptionInfoAsync()
        {
            var response = await Rest.GetAsync(GetUrl("/subscription"), new RestParameters(client.DefaultRequestHeaders));
            response.ValidateResponse();
            return JsonConvert.DeserializeObject<SubscriptionInfo>(response.ResponseBody, client.JsonSerializationOptions);
        }
    }
}
