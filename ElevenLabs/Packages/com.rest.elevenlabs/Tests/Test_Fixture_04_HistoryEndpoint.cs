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
            var api = new ElevenLabsClient();
            Assert.NotNull(api.HistoryEndpoint);
            var results = await api.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(results);
            Assert.IsNotEmpty(results);

            foreach (var item in results.OrderBy(item => item.Date))
            {
                Debug.Log($"{item.State} {item.Date} | {item.Id} | {item.Text.Length} | {item.Text}");
            }
        }

        [Test]
        public async Task Test_02_GetHistoryAudio()
        {
            var api = new ElevenLabsClient();
            Assert.NotNull(api.HistoryEndpoint);
            var historyItems = await api.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(historyItems);
            Assert.IsNotEmpty(historyItems);
            var downloadItem = historyItems.OrderByDescending(item => item.Date).FirstOrDefault();
            Assert.NotNull(downloadItem);
            Debug.Log($"Downloading {downloadItem.Id}...");
            var result = await api.HistoryEndpoint.GetHistoryAudioAsync(downloadItem);
            Assert.NotNull(result);
        }

        [Test]
        public async Task Test_03_DownloadAllHistoryItems()
        {
            var api = new ElevenLabsClient();
            Assert.NotNull(api.HistoryEndpoint);
            var historyItems = await api.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(historyItems);
            Assert.IsNotEmpty(historyItems);
            var singleItem = historyItems.FirstOrDefault();
            var singleItemResult = await api.HistoryEndpoint.DownloadHistoryItemsAsync(new List<string> { singleItem });
            Assert.NotNull(singleItemResult);
            Assert.IsNotEmpty(singleItemResult);
            var downloadItems = historyItems.Select(item => item.Id).ToList();
            var results = await api.HistoryEndpoint.DownloadHistoryItemsAsync(downloadItems);
            Assert.NotNull(results);
            Assert.IsNotEmpty(results);
        }

        [Test]
        public async Task Test_04_DeleteHistoryItem()
        {
            var api = new ElevenLabsClient();
            Assert.NotNull(api.HistoryEndpoint);
            var historyItems = await api.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(historyItems);
            Assert.IsNotEmpty(historyItems);
            var itemToDelete = historyItems
                .OrderBy(item => item.Date)
                .FirstOrDefault();
            Assert.NotNull(itemToDelete);
            Debug.Log($"Deleting {itemToDelete.Id}...");
            var result = await api.HistoryEndpoint.DeleteHistoryItemAsync(itemToDelete);
            Assert.NotNull(result);
            Assert.IsTrue(result);
            var updatedItems = await api.HistoryEndpoint.GetHistoryAsync();
            Assert.NotNull(updatedItems);
            Assert.IsNotEmpty(updatedItems);
            var isDeleted = updatedItems.All(item => item.Id != itemToDelete.Id);
            Assert.IsTrue(isDeleted);

            foreach (var item in updatedItems.OrderBy(item => item.Date))
            {
                Debug.Log($"{item.State} {item.Date} | {item.Id} | {item.Text}");
            }
        }
    }
}
