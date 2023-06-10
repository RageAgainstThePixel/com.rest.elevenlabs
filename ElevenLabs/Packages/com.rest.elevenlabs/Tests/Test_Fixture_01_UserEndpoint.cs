// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System.Threading.Tasks;

namespace ElevenLabs.Voice.Tests
{
    internal class Test_Fixture_01_UserEndpoint
    {
        [Test]
        public async Task Test_01_GetUserInfo()
        {
            var api = new ElevenLabsClient(ElevenLabsAuthentication.Default.LoadFromEnvironment());
            Assert.NotNull(api.UserEndpoint);
            var result = await api.UserEndpoint.GetUserInfoAsync();
            Assert.NotNull(result);
        }

        [Test]
        public async Task Test_02_GetSubscriptionInfo()
        {
            var api = new ElevenLabsClient(ElevenLabsAuthentication.Default.LoadFromEnvironment());
            Assert.NotNull(api.UserEndpoint);
            var result = await api.UserEndpoint.GetSubscriptionInfoAsync();
            Assert.NotNull(result);
        }
    }
}
