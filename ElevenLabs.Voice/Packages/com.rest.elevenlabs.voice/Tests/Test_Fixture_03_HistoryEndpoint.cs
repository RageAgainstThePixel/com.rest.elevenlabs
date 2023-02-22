// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs;
using NUnit.Framework;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.TestTools;

namespace Rest.ElevenLabs.Voice.Tests
{
    internal class Test_Fixture_04_HistoryEndpoint
    {
        [UnityTest]
        public IEnumerator Test_01_GetHistory()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                var api = new ElevenLabsClient();
                Assert.NotNull(api.HistoryEndpoint);
                var result = await api.HistoryEndpoint.GetHistoryAsync();
                Assert.NotNull(result);
            });
        }

        [UnityTest]
        public IEnumerator Test_02_GetHistoryAudio()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                await Task.CompletedTask;
            });
        }

        [UnityTest]
        public IEnumerator Test_03_DeleteHistoryItem()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                await Task.CompletedTask;
            });
        }

        [UnityTest]
        public IEnumerator Test_04_DownloadAllHistoryItems()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                await Task.CompletedTask;
            });
        }
    }
}
