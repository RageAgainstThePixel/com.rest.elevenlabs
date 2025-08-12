// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Utilities.WebRequestRest;

namespace ElevenLabs.Voices
{
    public sealed class VoicesV2Endpoint : ElevenLabsBaseEndPoint
    {
        public VoicesV2Endpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "voices";

        protected override string ApiVersion => "v2";

        public async Task<VoiceList> GetVoicesAsync(VoiceQuery query = null, CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl(queryParameters: query?.ToQueryParams()), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<VoiceList>(response.Body, ElevenLabsClient.JsonSerializationOptions);
        }
    }
}
