// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace ElevenLabs
{
    /// <summary>
    /// Access to your history. Your history is a list of all your created audio including its metadata.
    /// </summary>
    public sealed class HistoryEndpoint : BaseEndPoint
    {
        public HistoryEndpoint(ElevenLabsClient api) : base(api) { }

        protected override string GetEndpoint() => $"{Api.BaseUrl}history";

        /// <summary>
        /// Get metadata about all your generated audio.
        /// </summary>
        public async Task GetHistoryAsync()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Get audio of a history item.
        /// </summary>
        /// <param name="id"></param>
        public async Task GetHistoryAudioAsync(string id)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Delete a history item by its id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteHistoryItem(string id)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Download one or more history items. If one history item ID is provided, we will return a single audio file.
        /// If more than one history item IDs are provided, we will provide the history items packed into a .zip file.
        /// </summary>
        /// <returns></returns>
        public async Task DownloadAllHistoryItemsAsync()
        {
            await Task.CompletedTask;
        }
    }
}
