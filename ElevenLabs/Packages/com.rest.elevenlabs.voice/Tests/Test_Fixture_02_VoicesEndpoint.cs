// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs;
using ElevenLabs.Voices;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
                var results = await api.VoicesEndpoint.GetAllVoicesAsync();
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
                var results = await api.VoicesEndpoint.GetAllVoicesAsync();
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
                var results = await api.VoicesEndpoint.GetAllVoicesAsync();
                Assert.NotNull(results);
                Assert.IsNotEmpty(results);
                var voice = results.FirstOrDefault();
                var result = await api.VoicesEndpoint.EditVoiceSettingsAsync(voice, new VoiceSettings(0.7f, 0.7f));
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
                var api = new ElevenLabsClient();
                Assert.NotNull(api.VoicesEndpoint);
                var testLabels = new Dictionary<string, string>
                {
                    { "accent", "american" }
                };
                var clipPath = AssetDatabase.GUIDToAssetPath("96e9fdf73bc7a944f93886694973b90e");
                var result = await api.VoicesEndpoint.AddVoiceAsync("Test Voice", new[] { clipPath }, testLabels);
                Assert.NotNull(result);
                Debug.Log($"{result.Name}");
                Assert.IsNotEmpty(result.Samples);
            });
        }

        [UnityTest]
        public IEnumerator Test_06_EditVoice()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                var api = new ElevenLabsClient();
                Assert.NotNull(api.VoicesEndpoint);
                var results = await api.VoicesEndpoint.GetAllVoicesAsync();
                Assert.NotNull(results);
                Assert.IsNotEmpty(results);
                var voiceToEdit = results.FirstOrDefault(voice => voice.Name.Contains("Test Voice"));
                Assert.NotNull(voiceToEdit);
                var testLabels = new Dictionary<string, string>
                {
                    { "accent", "american" },
                    { "key", "value" }
                };
                var clipPath = AssetDatabase.GUIDToAssetPath("96e9fdf73bc7a944f93886694973b90e");
                var result = await api.VoicesEndpoint.EditVoiceAsync(voiceToEdit, new[] { clipPath }, testLabels);
                Assert.NotNull(result);
                Assert.IsTrue(result);
            });
        }

        [UnityTest]
        public IEnumerator Test_07_GetVoiceSample()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                var api = new ElevenLabsClient();
                Assert.NotNull(api.VoicesEndpoint);
                var results = await api.VoicesEndpoint.GetAllVoicesAsync();
                Assert.NotNull(results);
                Assert.IsNotEmpty(results);
                var voice = results.FirstOrDefault(voice => voice.Name.Contains("Test Voice"));
                Assert.NotNull(voice);
                var updatedVoice = await api.VoicesEndpoint.GetVoiceAsync(voice);
                Assert.NotNull(updatedVoice);
                Assert.IsNotEmpty(updatedVoice.Samples);
                var sample = updatedVoice.Samples.FirstOrDefault();
                Assert.NotNull(sample);
                var result = await api.VoicesEndpoint.GetVoiceSampleAsync(updatedVoice, updatedVoice.Samples.FirstOrDefault());
                Assert.NotNull(result);
            });
        }

        [UnityTest]
        public IEnumerator Test_08_DeleteVoiceSample()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                var api = new ElevenLabsClient();
                Assert.NotNull(api.VoicesEndpoint);
                var results = await api.VoicesEndpoint.GetAllVoicesAsync();
                Assert.NotNull(results);
                Assert.IsNotEmpty(results);
                var voice = results.FirstOrDefault(voice => voice.Name.Contains("Test Voice"));
                Assert.NotNull(voice);
                var updatedVoice = await api.VoicesEndpoint.GetVoiceAsync(voice);
                Assert.NotNull(updatedVoice);
                Assert.IsNotEmpty(updatedVoice.Samples);
                var sample = updatedVoice.Samples.FirstOrDefault();
                Assert.NotNull(sample);
                var result = await api.VoicesEndpoint.DeleteVoiceSampleAsync(updatedVoice, sample);
                Assert.NotNull(result);
                Assert.IsTrue(result);
            });
        }

        [UnityTest]
        public IEnumerator Test_09_DeleteVoice()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                var api = new ElevenLabsClient();
                Assert.NotNull(api.VoicesEndpoint);
                var results = await api.VoicesEndpoint.GetAllVoicesAsync();
                Assert.NotNull(results);
                Assert.IsNotEmpty(results);
                var voicesToDelete = results.Where(voice => voice.Name.Contains("Test Voice")).ToList();
                Assert.NotNull(voicesToDelete);
                Assert.IsNotEmpty(voicesToDelete);

                foreach (var voice in voicesToDelete)
                {
                    var result = await api.VoicesEndpoint.DeleteVoiceAsync(voice);
                    Assert.NotNull(result);
                    Assert.IsTrue(result);
                }
            });
        }
    }
}
