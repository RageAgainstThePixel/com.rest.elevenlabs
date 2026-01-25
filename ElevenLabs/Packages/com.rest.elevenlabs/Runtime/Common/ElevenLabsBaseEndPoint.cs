// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Utilities.WebRequestRest;

namespace ElevenLabs
{
    public abstract class ElevenLabsBaseEndPoint : BaseEndPoint<ElevenLabsClient, ElevenLabsAuthentication, ElevenLabsSettings>
    {
        protected ElevenLabsBaseEndPoint(ElevenLabsClient client) : base(client) { }
        protected override string GetUrl(string endpoint = "", Dictionary<string, string> queryParameters = null)
            => GetEndpoint(client.Settings.BaseRequestUrlFormat, endpoint, queryParameters);

        protected string GetWebsocketUri(string endpoint = "", Dictionary<string, string> queryParameters = null)
            => GetEndpoint(client.Settings.BaseWebSocketUrlFormat, endpoint, queryParameters);

        private string GetEndpoint(string baseUrlFormat, string endpoint = "", Dictionary<string, string> queryParameters = null)
        {
            var result = string.Format(baseUrlFormat, ApiVersion, $"{Root}{endpoint}");

            foreach (var defaultQueryParameter in client.Settings.Info.DefaultQueryParameters)
            {
                queryParameters ??= new Dictionary<string, string>();
                queryParameters.Add(defaultQueryParameter.Key, defaultQueryParameter.Value);
            }

            if (queryParameters is { Count: not 0 })
            {
                result += $"?{string.Join('&', queryParameters.Select(parameter => $"{parameter.Key}={parameter.Value}"))}";
            }

            return result;
        }
    }
}
