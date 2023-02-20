// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;

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
        public async Task GetUserInfoAsync()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets your subscription info.
        /// </summary>
        public async Task GetSubscriptionInfoAsync()
        {
            await Task.CompletedTask;
        }
    }
}
