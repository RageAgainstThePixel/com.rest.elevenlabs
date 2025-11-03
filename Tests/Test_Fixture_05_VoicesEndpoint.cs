// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.VoiceGeneration;
using ElevenLabs.Voices;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ElevenLabs.Tests
{
    internal class Test_Fixture_05_VoicesEndpoint : AbstractTestFixture
    {
        [Test]
        public async Task Test_01_01_GetVoices()
        {
            Assert.NotNull(ElevenLabsClient.VoicesEndpoint);
            var results = await ElevenLabsClient.VoicesEndpoint.GetAllVoicesAsync();
            Assert.NotNull(results);
            Assert.IsNotEmpty(results);

            foreach (var voice in results)
            {
                Debug.Log($"{voice.Id} | {voice.Name} | similarity boost: {voice.Settings?.SimilarityBoost} | stability: {voice.Settings?.Stability}");
            }
        }

        [Test]
        public async Task Test_01_02_GetSharedVoices()
        {
            Assert.NotNull(ElevenLabsClient.SharedVoicesEndpoint);
            var query = new SharedVoiceQuery { Accent = "american" };
            var results = await ElevenLabsClient.SharedVoicesEndpoint.GetSharedVoicesAsync(query);
            Assert.NotNull(results);
            Assert.IsNotEmpty(results.Voices);

            Debug.Log($"{results.Voices.Count} | has more? {results.HasMore} | {results.LastId}");

            foreach (var voice in results.Voices)
            {
                Debug.Log($"{voice.VoiceId} | {voice.Date} | {voice.Name} | {voice.Accent}");
                Assert.AreEqual("american", voice.Accent);
            }
        }

        [Test]
        public async Task Test_02_GetDefaultVoiceSettings()
        {
            Assert.NotNull(ElevenLabsClient.VoicesEndpoint);
            var result = await ElevenLabsClient.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
            Assert.NotNull(result);
            Debug.Log($"stability: {result.Stability} | similarity boost: {result.SimilarityBoost}");
        }

        [Test]
        public async Task Test_03_GetVoice()
        {
            Assert.NotNull(ElevenLabsClient.VoicesEndpoint);
            var results = await ElevenLabsClient.VoicesEndpoint.GetAllVoicesAsync();
            Assert.NotNull(results);
            Assert.IsNotEmpty(results);
            var voiceToGet = results.OrderBy(voice => voice.Name).FirstOrDefault();
            var result = await ElevenLabsClient.VoicesEndpoint.GetVoiceAsync(voiceToGet);
            Assert.NotNull(result);
            Debug.Log($"{result.Id} | {result.Name} | {result.PreviewUrl}");
        }

        [Test]
        public async Task Test_04_EditVoiceSettings()
        {
            Assert.NotNull(ElevenLabsClient.VoicesEndpoint);
            var results = await ElevenLabsClient.VoicesEndpoint.GetAllVoicesAsync();
            Assert.NotNull(results);
            Assert.IsNotEmpty(results);
            var voice = results.FirstOrDefault();
            var result = await ElevenLabsClient.VoicesEndpoint.EditVoiceSettingsAsync(voice, new VoiceSettings(0.7f, 0.7f));
            Assert.NotNull(result);
            Assert.IsTrue(result);
            var updatedVoice = await ElevenLabsClient.VoicesEndpoint.GetVoiceAsync(voice);
            Assert.NotNull(updatedVoice);
            Debug.Log($"{updatedVoice.Id} | similarity boost: {updatedVoice.Settings?.SimilarityBoost} | stability: {updatedVoice.Settings?.Stability}");
            var defaultVoiceSettings = await ElevenLabsClient.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
            Assert.NotNull(defaultVoiceSettings);
            var defaultResult = await ElevenLabsClient.VoicesEndpoint.EditVoiceSettingsAsync(voice, defaultVoiceSettings);
            Assert.NotNull(defaultResult);
            Assert.IsTrue(defaultResult);
        }

        [Test]
        public async Task Test_05_AddVoice()
        {
            Assert.NotNull(ElevenLabsClient.VoicesEndpoint);
            var testLabels = new Dictionary<string, string>
            {
                { "accent", "american" }
            };
            var clipPath = AssetDatabase.GUIDToAssetPath("96e9fdf73bc7a944f93886694973b90e");
            var result = await ElevenLabsClient.VoicesEndpoint.AddVoiceAsync("Test Voice", new[] { clipPath }, testLabels);
            Assert.NotNull(result);
            Debug.Log($"{result.Name}");
            Assert.IsNotEmpty(result.Samples);
        }

        [Test]
        public async Task Test_06_EditVoice()
        {
            Assert.NotNull(ElevenLabsClient.VoicesEndpoint);
            var results = await ElevenLabsClient.VoicesEndpoint.GetAllVoicesAsync();
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
            var result = await ElevenLabsClient.VoicesEndpoint.EditVoiceAsync(voiceToEdit, new[] { clipPath }, testLabels);
            Assert.NotNull(result);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task Test_07_GetVoiceSample()
        {
            Assert.NotNull(ElevenLabsClient.VoicesEndpoint);
            var results = await ElevenLabsClient.VoicesEndpoint.GetAllVoicesAsync();
            Assert.NotNull(results);
            Assert.IsNotEmpty(results);
            var voice = results.FirstOrDefault(voice => voice.Name.Contains("Test Voice"));
            Assert.NotNull(voice);
            var updatedVoice = await ElevenLabsClient.VoicesEndpoint.GetVoiceAsync(voice);
            Assert.NotNull(updatedVoice);
            Assert.IsNotEmpty(updatedVoice.Samples);
            var sample = updatedVoice.Samples.FirstOrDefault();
            Assert.NotNull(sample);
            var result = await ElevenLabsClient.VoicesEndpoint.DownloadVoiceSampleAudioAsync(updatedVoice, sample);
            Assert.NotNull(result);
        }

        [Test]
        public async Task Test_08_DeleteVoiceSample()
        {
            Assert.NotNull(ElevenLabsClient.VoicesEndpoint);
            var results = await ElevenLabsClient.VoicesEndpoint.GetAllVoicesAsync();
            Assert.NotNull(results);
            Assert.IsNotEmpty(results);
            var voice = results.FirstOrDefault(voice => voice.Name.Contains("Test Voice"));
            Assert.NotNull(voice);
            var updatedVoice = await ElevenLabsClient.VoicesEndpoint.GetVoiceAsync(voice);
            Assert.NotNull(updatedVoice);
            Assert.IsNotEmpty(updatedVoice.Samples);
            var sample = updatedVoice.Samples.FirstOrDefault();
            Assert.NotNull(sample);
            var result = await ElevenLabsClient.VoicesEndpoint.DeleteVoiceSampleAsync(updatedVoice, sample);
            Assert.NotNull(result);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task Test_09_DeleteVoice()
        {
            Assert.NotNull(ElevenLabsClient.VoicesEndpoint);
            var results = await ElevenLabsClient.VoicesEndpoint.GetAllVoicesAsync();
            Assert.NotNull(results);
            Assert.IsNotEmpty(results);
            var voicesToDelete = results.Where(voice => voice.Name.Contains("Test Voice")).ToList();
            Assert.NotNull(voicesToDelete);
            Assert.IsNotEmpty(voicesToDelete);

            foreach (var voice in voicesToDelete)
            {
                var result = await ElevenLabsClient.VoicesEndpoint.DeleteVoiceAsync(voice);
                Assert.NotNull(result);
                Assert.IsTrue(result);
            }
        }

        [Test]
        public async Task Test_10_IterateDefaultVoices()
        {
            Assert.NotNull(ElevenLabsClient.VoicesV2Endpoint);
            var voices = new List<Voice>();
            var query = new VoiceQuery(voiceType: VoiceTypes.Default, pageSize: 10);
            int? previousTotalCount = null;

            do
            {
                var page = await ElevenLabsClient.VoicesV2Endpoint.GetVoicesAsync(query);

                if (page.HasMore)
                {
                    Assert.AreEqual(query.PageSize, page.Voices.Count);
                }

                if (previousTotalCount != null)
                {
                    Assert.AreEqual(previousTotalCount, page.TotalCount);
                }

                previousTotalCount = page.TotalCount;
                voices.AddRange(page.Voices);
                query = query.WithNextPageToken(page.NextPageToken);
            } while (!string.IsNullOrWhiteSpace(query.NextPageToken));

            Assert.NotNull(voices);
            Assert.IsNotEmpty(voices);
            Assert.AreEqual(previousTotalCount, voices.Count);

            foreach (var voice in voices)
            {
                Debug.Log($"{voice.Id} | {voice.Name}");
            }
        }
    }
}
