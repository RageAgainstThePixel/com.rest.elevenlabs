// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.SharpZipLib.Zip;
using UnityEngine;
using Utilities.WebRequestRest;

namespace ElevenLabs.History
{
    /// <summary>
    /// Access to your history. Your history is a list of all your created audio including its metadata.
    /// </summary>
    public sealed class HistoryEndpoint : BaseEndPoint
    {
        private class HistoryInfo
        {
            [JsonConstructor]
            public HistoryInfo([JsonProperty(nameof(History))] List<HistoryItem> history)
            {
                History = history;
            }

            [JsonProperty(nameof(History))]
            public IReadOnlyList<HistoryItem> History { get; }
        }

        public HistoryEndpoint(ElevenLabsClient api) : base(api) { }

        protected override string GetEndpoint()
            => $"{Api.BaseUrl}history";

        /// <summary>
        /// Get metadata about all your generated audio.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A list of history items containing metadata about generated audio.</returns>
        public async Task<IReadOnlyList<HistoryItem>> GetHistoryAsync(CancellationToken cancellationToken = default)
        {
            var result = await Api.Client.GetAsync($"{GetEndpoint()}", cancellationToken);
            var resultAsString = await result.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<HistoryInfo>(resultAsString, Api.JsonSerializationOptions)?.History;
        }

        /// <summary>
        /// Get audio of a history item.
        /// </summary>
        /// <param name="historyItem"><see cref="HistoryItem.Id"/></param>
        /// <param name="saveDirectory">Optional, save directory for the downloaded <see cref="AudioClip"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="AudioClip"/>.</returns>
        public async Task<AudioClip> GetHistoryAudioAsync(HistoryItem historyItem, string saveDirectory = null, CancellationToken cancellationToken = default)
        {
            Rest.ValidateCacheDirectory();

            var rootDirectory = (saveDirectory ?? Rest.DownloadCacheDirectory).CreateNewDirectory(nameof(ElevenLabs));
            var downloadDirectory = rootDirectory.CreateNewDirectory(nameof(History));
            var voiceDirectory = downloadDirectory.CreateNewDirectory(historyItem.VoiceName);
            var filePath = Path.Combine(voiceDirectory, $"{historyItem.Id}.mp3");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var response = await Api.Client.GetAsync($"{GetEndpoint()}/{historyItem.Id}/audio", cancellationToken);
            await response.CheckResponseAsync(cancellationToken);

            var responseStream = await response.Content.ReadAsStreamAsync();

            try
            {
                var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);

                try
                {
                    await responseStream.CopyToAsync(fileStream, cancellationToken);
                    await fileStream.FlushAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    fileStream.Close();
                    await fileStream.DisposeAsync();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                await responseStream.DisposeAsync();
            }

            var audioClip = await Rest.DownloadAudioClipAsync($"file://{filePath}", AudioType.MPEG, cancellationToken: cancellationToken);
            return audioClip;
        }

        /// <summary>
        /// Delete a history item by its id.
        /// </summary>
        /// <param name="historyId"><see cref="HistoryItem.Id"/></param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if history item was successfully deleted.</returns>
        public async Task<bool> DeleteHistoryItemAsync(string historyId, CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.DeleteAsync($"{GetEndpoint()}/{historyId}", cancellationToken);
            await response.ReadAsStringAsync(true);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Download one or more history items. If one history item ID is provided, we will return a single audio file.
        /// If more than one history item IDs are provided multiple audio files will be downloaded.
        /// </summary>
        /// <param name="historyItemIds">One or more history item ids queued for download.</param>
        /// <param name="saveDirectory">Optional, directory path to save the history in.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A list of Audio Clips downloaded by the request.</returns>
        public async Task<IReadOnlyList<AudioClip>> DownloadHistoryItemsAsync(List<string> historyItemIds, string saveDirectory = null, CancellationToken cancellationToken = default)
        {
            if (historyItemIds is not { Count: not 0 })
            {
                throw new ArgumentOutOfRangeException(nameof(historyItemIds));
            }

            var audioClips = new ConcurrentBag<AudioClip>();

            if (historyItemIds.Count == 1)
            {
                var history = await GetHistoryAsync(cancellationToken);
                var historyItem = history.FirstOrDefault(item => item.Id == historyItemIds.FirstOrDefault());
                audioClips.Add(await GetHistoryAudioAsync(historyItem, saveDirectory, cancellationToken));
            }
            else
            {
                var jsonContent = $"{{\"history_item_ids\":[\"{string.Join("\",\"", historyItemIds)}\"]}}".ToJsonStringContent();
                var response = await Api.Client.PostAsync($"{GetEndpoint()}/download", jsonContent, cancellationToken);
                await response.CheckResponseAsync(cancellationToken);
                var unZipTasks = new List<Task>();
                var responseStream = await response.Content.ReadAsStreamAsync();

                try
                {
                    var zipFile = new ZipFile(responseStream);

                    if (saveDirectory == null)
                    {
                        Rest.ValidateCacheDirectory();
                    }

                    var rootDirectory = Path.Combine(saveDirectory ?? Rest.DownloadCacheDirectory, nameof(ElevenLabs));

                    if (!Directory.Exists(rootDirectory))
                    {
                        Directory.CreateDirectory(rootDirectory);
                    }

                    var downloadDirectory = Path.Combine(rootDirectory, nameof(History));

                    Directory.CreateDirectory(downloadDirectory);

                    foreach (ZipEntry entry in zipFile)
                    {
                        unZipTasks.Add(Task.Run(UnZipAudioClipAsync, cancellationToken));

                        async Task UnZipAudioClipAsync()
                        {
                            var filePath = Path.Combine(downloadDirectory, entry.Name);

                            if (File.Exists(filePath))
                            {
                                return;
                            }

                            var parentDirectory = Directory.GetParent(filePath)!;

                            if (!parentDirectory.Exists)
                            {
                                Directory.CreateDirectory(parentDirectory.FullName);
                            }

                            var itemStream = zipFile.GetInputStream(entry);

                            try
                            {
                                var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);

                                try
                                {
                                    await itemStream.CopyToAsync(fileStream, cancellationToken);
                                    await fileStream.FlushAsync(cancellationToken);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError(e);
                                }
                                finally
                                {
                                    fileStream.Close();
                                    await fileStream.DisposeAsync();
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e);
                            }
                            finally
                            {
                                itemStream.Close();
                                await itemStream.DisposeAsync();
                            }

                            var audioClip = await Rest.DownloadAudioClipAsync($"file://{filePath}", AudioType.MPEG, cancellationToken: cancellationToken);
                            audioClips.Add(audioClip);
                        }
                    }

                    await Task.WhenAll(unZipTasks);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    await responseStream.DisposeAsync();
                }
            }

            return audioClips.ToList();
        }
    }
}
