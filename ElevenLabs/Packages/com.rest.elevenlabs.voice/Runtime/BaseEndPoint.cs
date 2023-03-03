// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace ElevenLabs
{
    public abstract class BaseEndPoint
    {
        protected readonly ElevenLabsClient Api;

        /// <summary>
        /// Constructor of the api endpoint.
        /// Rather than instantiating this yourself, access it through an instance of <see cref="ElevenLabsClient"/>.
        /// </summary>
        internal BaseEndPoint(ElevenLabsClient api) => Api = api;

        /// <summary>
        /// Gets the basic endpoint url for the API
        /// </summary>
        /// <returns>The completed basic url for the endpoint.</returns>
        protected abstract string GetEndpoint();
    }
}
