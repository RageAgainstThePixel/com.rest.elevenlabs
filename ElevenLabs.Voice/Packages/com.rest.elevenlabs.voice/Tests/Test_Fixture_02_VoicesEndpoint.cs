// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs;
using ElevenLabs.Voices;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rest.ElevenLabs.Voice.Tests
{
    internal class Test_Fixture_02_VoicesEndpoint
    {
        [UnityTest]
        public IEnumerator Test_01_GetVoices()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                var api = new ElevenLabsClient();
                Assert.NotNull(api.VoicesEndpoint);
                var results = await api.VoicesEndpoint.GetVoicesAsync();
                Assert.NotNull(results);
                Assert.IsNotEmpty(results);

                foreach (var voice in results)
                {
                    Debug.Log($"{voice.Id} | {voice.Name} | similarity boost: {voice.Settings?.SimilarityBoost} | stability: {voice.Settings?.Stability}");
                }
            });
        }

        [UnityTest]
        public IEnumerator Test_02_GetDefaultVoiceSettings()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                var api = new ElevenLabsClient();
                Assert.NotNull(api.VoicesEndpoint);
                var result = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
                Assert.NotNull(result);
                Debug.Log($"stability: {result.Stability} | similarity boost: {result.SimilarityBoost}");
            });
        }

        [UnityTest]
        public IEnumerator Test_03_GetVoice()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                var api = new ElevenLabsClient();
                Assert.NotNull(api.VoicesEndpoint);
                var results = await api.VoicesEndpoint.GetVoicesAsync();
                Assert.NotNull(results);
                Assert.IsNotEmpty(results);
                var voiceToGet = results.OrderBy(voice => voice.Name).FirstOrDefault();
                var result = await api.VoicesEndpoint.GetVoiceAsync(voiceToGet);
                Assert.NotNull(result);
                Debug.Log($"{result.Id} | {result.Name} | {result.PreviewUrl}");
            });
        }

        [UnityTest]
        public IEnumerator Test_04_EditVoiceSettings()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                var api = new ElevenLabsClient();
                Assert.NotNull(api.VoicesEndpoint);
                var results = await api.VoicesEndpoint.GetVoicesAsync();

                var voice = results.FirstOrDefault();
                var result = await api.VoicesEndpoint.EditVoiceSettingsAsync(voice, new VoiceSettings(0.7, 0.7));
                Assert.NotNull(result);
                Assert.IsTrue(result);
                var updatedVoice = await api.VoicesEndpoint.GetVoiceAsync(voice);
                Assert.NotNull(updatedVoice);
                Debug.Log($"{updatedVoice.Id} | similarity boost: {updatedVoice.Settings?.SimilarityBoost} | stability: {updatedVoice.Settings?.Stability}");
                var defaultVoiceSettings = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
                Assert.NotNull(defaultVoiceSettings);
                var defaultResult = await api.VoicesEndpoint.EditVoiceSettingsAsync(voice, defaultVoiceSettings);
                Assert.NotNull(defaultResult);
                Assert.IsTrue(defaultResult);
            });
        }

        [UnityTest]
        public IEnumerator Test_05_AddVoice()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                await Task.CompletedTask;
            });
        }

        [UnityTest]
        public IEnumerator Test_06_EditVoice()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                await Task.CompletedTask;
            });
        }

        [UnityTest]
        public IEnumerator Test_07_GetVoiceSample()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                await Task.CompletedTask;
            });
        }

        [UnityTest]
        public IEnumerator Test_08_DeleteVoiceSample()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                await Task.CompletedTask;
            });
        }

        [UnityTest]
        public IEnumerator Test_09_DeleteVoice()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                await Task.CompletedTask;
            });
        }
    }
}
