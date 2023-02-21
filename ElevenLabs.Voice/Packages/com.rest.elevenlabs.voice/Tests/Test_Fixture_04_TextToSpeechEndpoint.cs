// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Threading.Tasks;
using UnityEngine.TestTools;

namespace Rest.ElevenLabs.Voice.Tests
{
    internal class Test_Fixture_04_TextToSpeechEndpoint
    {
        [UnityTest]
        public IEnumerator Test_01_TextToSpeech()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                await Task.CompletedTask;
            });
        }

        [UnityTest]
        public IEnumerator Test_02_TextToSpeechStream()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                await Task.CompletedTask;
            });
        }
    }
}
