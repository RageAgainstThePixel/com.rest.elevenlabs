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
using Utilities.Async;
using Utilities.Encoding.Wav;
using Utilities.WebRequestRest;

namespace ElevenLabs.History
{
    /// <summary>
    /// Access to your history. Your history is a list of all your created audio including its metadata.
    /// </summary>
    public sealed class HistoryEndpoint : ElevenLabsBaseEndPoint
    {
        public HistoryEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "history";

        /// <summary>
        /// Get metadata about all your generated audio.
        /// </summary>
        /// <param name="pageSize">
        /// Optional, number of items to return. Cannot exceed 1000.<br/>
        /// Default: 100
        /// </param>
        /// <param name="startAfterId">Optional, the id of the item to start after.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="HistoryInfo"/>.</returns>
        public async Task<HistoryInfo> GetHistoryAsync(int? pageSize = null, string startAfterId = null, CancellationToken cancellationToken = default)
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
            return JsonConvert.DeserializeObject<HistoryInfo>(response.Body, ElevenLabsClient.JsonSerializationOptions);
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
        /// Download audio of a history item.
        /// </summary>
        /// <param name="historyItem"><see cref="HistoryItem"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceClip"/>.</returns>
        public async Task<VoiceClip> DownloadHistoryAudioAsync(HistoryItem historyItem, CancellationToken cancellationToken = default)
        {
            await Rest.ValidateCacheDirectoryAsync();
            var voiceDirectory = Rest.DownloadCacheDirectory
                .CreateNewDirectory(nameof(ElevenLabs))
                .CreateNewDirectory(nameof(History))
                .CreateNewDirectory(historyItem.VoiceId);
            var voice = await client.VoicesEndpoint.GetVoiceAsync(historyItem.VoiceId, cancellationToken: cancellationToken);
            var audioType = historyItem.ContentType.Contains("mpeg") ? AudioType.MPEG : AudioType.WAV;
            var extension = audioType switch
            {
                AudioType.MPEG => "mp3",
                AudioType.WAV => "wav",
                _ => throw new ArgumentOutOfRangeException($"Unsupported {nameof(AudioType)}: {audioType}")
            };
            var cachedPath = Path.Combine(voiceDirectory, $"{historyItem.Id}.{extension}");

            if (!File.Exists(cachedPath))
            {
                var response = await Rest.GetAsync(GetUrl($"/{historyItem.Id}/audio"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
                response.Validate(EnableDebug);

                switch (audioType)
                {
                    case AudioType.MPEG:
                        await File.WriteAllBytesAsync(cachedPath, response.Data, cancellationToken).ConfigureAwait(false);
                        break;
                    case AudioType.WAV:
                        var sampleRate = 44100; // TODO unknown sample rate. how do we figure it out?
                        await WavEncoder.WriteToFileAsync(cachedPath, response.Data, 1, sampleRate, cancellationToken: cancellationToken);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Unsupported {nameof(AudioType)}: {audioType}");
                }
            }

            await Awaiters.UnityMainThread;
            var audioClip = await Rest.DownloadAudioClipAsync(
                url: $"file://{cachedPath}",
                audioType: audioType,
                parameters: new RestParameters(debug: EnableDebug),
                cancellationToken: cancellationToken);
            return new VoiceClip(historyItem.Id, historyItem.Text, voice, audioClip, cachedPath);
        }

        /// <summary>
        /// Delete a history item by its id.
        /// </summary>
        /// <param name="id"><see cref="HistoryItem.Id"/> or <see cref="VoiceClip.Id"/>.</param>
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
        /// If no ids are specified, then the last 100 history items are downloaded.<br/>
        /// If one history item id is provided, we will return a single audio file.<br/>
        /// If more than one history item ids are provided multiple audio files will be downloaded.
        /// </summary>
        /// <param name="historyItemIds">Optional, One or more history item ids queued for download.</param>
        /// <param name="progress">Optional, <see cref="IProgress{T}"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A list of voice clips downloaded by the request.</returns>
        public async Task<IReadOnlyList<VoiceClip>> DownloadHistoryItemsAsync(List<string> historyItemIds = null, IProgress<string> progress = null, CancellationToken cancellationToken = default)
        {
            historyItemIds ??= (await GetHistoryAsync(cancellationToken: cancellationToken)).HistoryItems.Select(item => item.Id).ToList();
            var voiceClips = new ConcurrentBag<VoiceClip>();

            async Task DownloadItem(string historyItemId)
            {
                try
                {
                    await Awaiters.UnityMainThread;
                    var historyItem = await GetHistoryItemAsync(historyItemId, cancellationToken);
                    var voiceClip = await DownloadHistoryAudioAsync(historyItem, cancellationToken);
                    voiceClips.Add(voiceClip);
                    progress?.Report(historyItem.Id);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            await Task.WhenAll(historyItemIds.Select(DownloadItem)).ConfigureAwait(true);
            return voiceClips.ToList();
        }
    }
}
