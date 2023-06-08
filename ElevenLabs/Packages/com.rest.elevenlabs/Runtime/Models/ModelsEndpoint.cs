// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.WebRequestRest;

namespace ElevenLabs.Models
{
    public sealed class ModelsEndpoint : ElevenLabsBaseEndPoint
    {
        public ModelsEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "models";

        /// <summary>
        /// Access the different models available to the platform.
        /// </summary>
        /// <returns>A list of <see cref="Model"/>s you can use.</returns>
        public async Task<IReadOnlyList<Model>> GetModelsAsync()
        {
            var response = await Rest.GetAsync(GetUrl(), new RestParameters(client.DefaultRequestHeaders));
            response.ValidateResponse();
            return JsonConvert.DeserializeObject<IReadOnlyList<Model>>(response.ResponseBody, client.JsonSerializationOptions);
        }
    }
}
