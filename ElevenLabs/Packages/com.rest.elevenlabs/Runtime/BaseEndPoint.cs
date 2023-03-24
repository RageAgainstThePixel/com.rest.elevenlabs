// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace ElevenLabs
{
    public abstract class BaseEndPoint
    {
        internal BaseEndPoint(ElevenLabsClient api) => Api = api;

        protected readonly ElevenLabsClient Api;

        /// <summary>
        /// The root endpoint address.
        /// </summary>
        protected abstract string Root { get; }

        /// <summary>
        /// Gets the full formatted url for the API endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint url.</param>
        protected string GetUrl(string endpoint = "")
            => string.Format(Api.ElevenLabsClientSettings.BaseRequestUrlFormat, $"{Root}{endpoint}");
    }
}
