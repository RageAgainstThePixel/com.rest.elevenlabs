// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Threading.Tasks;
using UnityEngine.TestTools;

namespace Rest.ElevenLabs.Voice.Tests
{
    internal class Test_Fixture_03_HistoryEndpoint
    {
        [UnityTest]
        public IEnumerator Test_O1_GetHistory()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                await Task.CompletedTask;
            });
        }

        [UnityTest]
        public IEnumerator Test_O2_GetHistoryAudio()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                await Task.CompletedTask;
            });
        }

        [UnityTest]
        public IEnumerator Test_O3_DeleteHistoryItem()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                await Task.CompletedTask;
            });
        }

        [UnityTest]
        public IEnumerator Test_O4_DownloadAllHistoryItems()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                await Task.CompletedTask;
            });
        }
    }
}
