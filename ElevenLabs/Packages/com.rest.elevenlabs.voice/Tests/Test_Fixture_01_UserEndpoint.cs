// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

namespace Rest.ElevenLabs.Voice.Tests
{
    internal class Test_Fixture_01_UserEndpoint
    {
        [UnityTest]
        public IEnumerator Test_01_GetUserInfo()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                var api = new ElevenLabsClient();
                Assert.NotNull(api.UserEndpoint);
                var result = await api.UserEndpoint.GetUserInfoAsync();
                Assert.NotNull(result);
            });
        }

        [UnityTest]
        public IEnumerator Test_02_GetSubscriptionInfo()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                var api = new ElevenLabsClient();
                Assert.NotNull(api.UserEndpoint);
                var result = await api.UserEndpoint.GetSubscriptionInfoAsync();
                Assert.NotNull(result);
            });
        }
    }
}
