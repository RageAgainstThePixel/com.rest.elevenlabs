// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Utilities.Async;
using Utilities.WebRequestRest;

namespace ElevenLabs.History
{
    /// <summary>
    /// Access to your history. Your history is a list of all your created audio including its metadata.
    /// </summary>
    public sealed class HistoryEndpoint : ElevenLabsBaseEndPoint
    {
        [Preserve]
        private class HistoryInfo
        {
            [Preserve]
            [JsonConstructor]
            public HistoryInfo(
                [JsonProperty("history")] List<HistoryItem> history,
                [JsonProperty("last_history_item_id")] string lastHistoryItemId,
                [JsonProperty("has_more")] bool hasMore)
            {
                History = history;
                LastHistoryItemId = lastHistoryItemId;
                HasMore = hasMore;
            }

            [Preserve]
            [JsonProperty("history")]
            public IReadOnlyList<HistoryItem> History { get; }

            [Preserve]
            [JsonProperty("last_history_item_id")]
            public string LastHistoryItemId { get; }

            [Preserve]
            [JsonProperty("has_more")]
            public bool HasMore { get; }
        }

        public HistoryEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "history";

        /// <summary>
        /// Get metadata about all your generated audio.
        /// </summary>
        /// <param name="pageSize">Optional, number of items to return. Cannot exceed 1000.</param>
        /// <param name="startAfterId">Optional, the id of the item to start after.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A list of history items containing metadata about generated audio.</returns>
        public async Task<IReadOnlyList<HistoryItem>> GetHistoryAsync(int? pageSize = null, string startAfterId = null, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, string>();

            if (pageSize.HasValue)
            {
                parameters.Add("page_size", pageSize.ToString());
            }

            if (!string.IsNullOrWhiteSpace(startAfterId))
            {
                parameters.Add("start_after_history_item_id", startAfterId);
            }

            var response = await Rest.GetAsync(GetUrl(queryParameters: parameters), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<HistoryInfo>(response.Body, ElevenLabsClient.JsonSerializationOptions)?.History;
        }

        /// <summary>
        /// Gets a history item by id.
        /// </summary>
        /// <param name="id"><see cref="HistoryItem.Id"/> or <see cref="VoiceClip.Id"/></param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="HistoryItem"/></returns>
        public async Task<HistoryItem> GetHistoryItemAsync(string id, CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl($"/{id}"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<HistoryItem>(response.Body, ElevenLabsClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Get audio of a history item.
        /// </summary>
        /// <param name="historyItem"><see cref="HistoryItem"/></param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="AudioClip"/>.</returns>
        public async Task<VoiceClip> DownloadHistoryAudioAsync(HistoryItem historyItem, CancellationToken cancellationToken = default)
        {
            await Rest.ValidateCacheDirectoryAsync();
            var voiceDirectory = Rest.DownloadCacheDirectory
                .CreateNewDirectory(nameof(ElevenLabs))
                .CreateNewDirectory(nameof(History))
                .CreateNewDirectory(historyItem.VoiceId);

            // TODO set file extension based on historyItem.MimeType
            var cachedPath = Path.Combine(voiceDirectory, $"{historyItem.Id}.mp3");

            if (!File.Exists(cachedPath))
            {
                var response = await Rest.GetAsync(GetUrl($"/{historyItem.Id}/audio"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
                response.Validate(EnableDebug);
                // TODO write file based on historyItem.MimeType
                var responseStream = new MemoryStream(response.Data);
                var fileStream = new FileStream(cachedPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);

                try
                {
                    await responseStream.CopyToAsync(fileStream, cancellationToken);
                    await fileStream.FlushAsync(cancellationToken);
                }
                finally
                {
                    fileStream.Close();
                    await fileStream.DisposeAsync();
                    await responseStream.DisposeAsync();
                }
            }

            var voice = await client.VoicesEndpoint.GetVoiceAsync(historyItem.VoiceId, true, cancellationToken);
            var audioClip = await Rest.DownloadAudioClipAsync($"file://{cachedPath}", AudioType.MPEG, cancellationToken: cancellationToken);
            return new VoiceClip(historyItem.Id, historyItem.Text, voice, audioClip, cachedPath);
        }

        /// <summary>
        /// Delete a history item by its id.
        /// </summary>
        /// <param name="id"><see cref="HistoryItem.Id"/> or <see cref="VoiceClip.Id"/></param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if history item was successfully deleted.</returns>
        public async Task<bool> DeleteHistoryItemAsync(string id, CancellationToken cancellationToken = default)
        {
            var response = await Rest.DeleteAsync(GetUrl($"/{id}"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return response.Successful;
        }

        /// <summary>
        /// Download one or more history items.<br/>
        /// If no ids are specified, then all history items are downloaded.<br/>
        /// If one history item id is provided, we will return a single audio file.<br/>
        /// If more than one history item ids are provided multiple audio files will be downloaded.
        /// </summary>
        /// <param name="historyItemIds">Optional, One or more history item ids queued for download.</param>
        /// <param name="progress">Optional, <see cref="IProgress{T}"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A list of Audio Clips downloaded by the request.</returns>
        public async Task<IReadOnlyList<VoiceClip>> DownloadHistoryItemsAsync(List<string> historyItemIds = null, IProgress<string> progress = null, CancellationToken cancellationToken = default)
        {
            historyItemIds ??= (await GetHistoryAsync(cancellationToken: cancellationToken)).Select(item => item.Id).ToList();
            var historyItems = new ConcurrentBag<VoiceClip>();

            async Task DownloadItem(string historyItemId)
            {
                try
                {
                    await Awaiters.UnityMainThread;
                    var historyItem = await GetHistoryItemAsync(historyItemId, cancellationToken);
                    historyItems.Add(await DownloadHistoryAudioAsync(historyItem, cancellationToken));
                    progress?.Report(historyItem.Id);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            await Task.WhenAll(historyItemIds.Select(DownloadItem)).ConfigureAwait(true);
            return historyItems.ToList();
        }
    }
}
