// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ElevenLabs.Voice.Tests
{
    internal class Test_Fixture_04_HistoryEndpoint
    {
        [Test]
        public async Task Test_01_GetHistory()
        {
            var api = new ElevenLabsClient(ElevenLabsAuthentication.Default.LoadFromEnvironment());
            Assert.NotNull(api.HistoryEndpoint);
            var historyInfo = await api.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(historyInfo);
            Assert.IsNotEmpty(historyInfo.HistoryItems);

            foreach (var item in historyInfo.HistoryItems.OrderBy(item => item.Date))
            {
                Debug.Log($"{item.State} {item.Date} | {item.Id} | {item.Text.Length} | {item.Text}");
            }
        }

        [Test]
        public async Task Test_02_GetHistoryAudio()
        {
            var api = new ElevenLabsClient(ElevenLabsAuthentication.Default.LoadFromEnvironment());
            Assert.NotNull(api.HistoryEndpoint);
            var historyInfo = await api.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(historyInfo);
            Assert.IsNotEmpty(historyInfo.HistoryItems);
            var downloadItem = historyInfo.HistoryItems.OrderByDescending(item => item.Date).FirstOrDefault();
            Assert.NotNull(downloadItem);
            Debug.Log($"Downloading {downloadItem.Id}...");
            var voiceClip = await api.HistoryEndpoint.DownloadHistoryAudioAsync(downloadItem);
            Assert.NotNull(voiceClip);
        }

        [Test]
        public async Task Test_03_DownloadAllHistoryItems()
        {
            var api = new ElevenLabsClient(ElevenLabsAuthentication.Default.LoadFromEnvironment());
            Assert.NotNull(api.HistoryEndpoint);
            var historyInfo = await api.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(historyInfo);
            Assert.IsNotEmpty(historyInfo.HistoryItems);
            var singleItem = historyInfo.HistoryItems.FirstOrDefault();
            var singleItemResult = await api.HistoryEndpoint.DownloadHistoryItemsAsync(new List<string> { singleItem });
            Assert.NotNull(singleItemResult);
            Assert.IsNotEmpty(singleItemResult);
            var downloadItems = historyInfo.HistoryItems.Select(item => item.Id).ToList();
            var voiceClips = await api.HistoryEndpoint.DownloadHistoryItemsAsync(downloadItems);
            Assert.NotNull(voiceClips);
            Assert.IsNotEmpty(voiceClips);
        }

        [Test]
        public async Task Test_04_DeleteHistoryItem()
        {
            var api = new ElevenLabsClient(ElevenLabsAuthentication.Default.LoadFromEnvironment());
            Assert.NotNull(api.HistoryEndpoint);
            var historyInfo = await api.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(historyInfo);
            Assert.IsNotEmpty(historyInfo.HistoryItems);
            var itemsToDelete = historyInfo.HistoryItems.Where(item => item.Text.Contains("The quick brown fox jumps over the lazy dog.")).ToList();
            Assert.NotNull(itemsToDelete);
            Assert.IsNotEmpty(itemsToDelete);

            foreach (var historyItem in itemsToDelete)
            {
                Debug.Log($"Deleting {historyItem.Id}...");
                var result = await api.HistoryEndpoint.DeleteHistoryItemAsync(historyItem.Id);
                Assert.NotNull(result);
                Assert.IsTrue(result);
            }

            var updatedHistoryInfo = await api.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(updatedHistoryInfo);
            Assert.That(updatedHistoryInfo, Has.None.EqualTo(itemsToDelete));

            foreach (var item in updatedHistoryInfo.HistoryItems.OrderBy(item => item.Date))
            {
                Debug.Log($"{item.State} {item.Date} | {item.Id} | {item.Text}");
            }
        }
    }
}
