// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
            public HistoryInfo(
                [JsonProperty("history")] List<HistoryItem> history)
            {
                History = history;
            }

            [JsonProperty("history")]
            public IReadOnlyList<HistoryItem> History { get; }
        }

        public HistoryEndpoint(ElevenLabsClient api) : base(api) { }

        protected override string GetEndpoint()
            => $"{Api.BaseUrl}history";

        /// <summary>
        /// Get metadata about all your generated audio.
        /// </summary>
        public async Task<IReadOnlyList<HistoryItem>> GetHistoryAsync(CancellationToken cancellationToken = default)
        {
            var result = await Api.Client.GetAsync($"{GetEndpoint()}", cancellationToken);
            var resultAsString = await result.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<HistoryInfo>(resultAsString, Api.JsonSerializationOptions)?.History;
        }

        /// <summary>
        /// Get audio of a history item.
        /// </summary>
        /// <param name="historyId"></param>
        /// <param name="cancellationToken"></param>
        public async Task GetHistoryAudioAsync(string historyId, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Delete a history item by its id.
        /// </summary>
        /// <param name="historyId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>True, if history item was successfully deleted.</returns>
        public async Task<bool> DeleteHistoryItemAsync(string historyId, CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.DeleteAsync($"{GetEndpoint()}/{historyId}", cancellationToken);
            await response.ReadAsStringAsync(true);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Download one or more history items. If one history item ID is provided, we will return a single audio file.
        /// If more than one history item IDs are provided, we will provide the history items packed into a .zip file.
        /// </summary>
        /// <returns></returns>
        public async Task DownloadAllHistoryItemsAsync(IEnumerable<string> historyItemIds = null, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }
    }
}
