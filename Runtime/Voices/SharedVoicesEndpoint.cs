// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Utilities.WebRequestRest;

namespace ElevenLabs.Voices
{
    public sealed class SharedVoicesEndpoint : ElevenLabsBaseEndPoint
    {
        public SharedVoicesEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "shared-voices";

        /// <summary>
        /// Gets a list of shared voices.
        /// </summary>
        /// <param name="query">Optional, <see cref="SharedVoiceQuery"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="SharedVoiceList"/>.</returns>
        public async Task<SharedVoiceList> GetSharedVoicesAsync(SharedVoiceQuery query = null, CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl(queryParameters: query?.ToQueryParams()), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<SharedVoiceList>(response.Body, ElevenLabsClient.JsonSerializationOptions);
        }
    }
}
