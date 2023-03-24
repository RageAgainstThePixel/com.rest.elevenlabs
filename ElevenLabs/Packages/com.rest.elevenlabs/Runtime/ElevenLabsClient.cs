// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.History;
using ElevenLabs.TextToSpeech;
using ElevenLabs.User;
using ElevenLabs.VoiceGeneration;
using ElevenLabs.Voices;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Authentication;

namespace ElevenLabs
{
    public sealed class ElevenLabsClient
    {
        /// <summary>
        /// Creates a new client for the Eleven Labs API, handling auth and allowing for access to various API endpoints.
        /// </summary>
        /// <param name="elevenLabsAuthentication">The API authentication information to use for API calls,
        /// or <see langword="null"/> to attempt to use the <see cref="ElevenLabs.ElevenLabsAuthentication.Default"/>,
        /// potentially loading from environment vars or from a config file.</param>
        /// <param name="clientSettings">Optional, <see cref="ElevenLabsClientSettings"/> for specifying a proxy domain.</param>
        /// <param name="httpClient">Optional, <see cref="HttpClient"/>.</param>
        /// <exception cref="AuthenticationException">Raised when authentication details are missing or invalid.</exception>
        public ElevenLabsClient(ElevenLabsAuthentication elevenLabsAuthentication = null, ElevenLabsClientSettings clientSettings = null, HttpClient httpClient = null)
        {
            ElevenLabsAuthentication = elevenLabsAuthentication ?? ElevenLabsAuthentication.Default;
            ElevenLabsClientSettings = clientSettings ?? ElevenLabsClientSettings.Default;

            if (string.IsNullOrWhiteSpace(ElevenLabsAuthentication?.ApiKey))
            {
                throw new AuthenticationException("You must provide API authentication.  Please refer to https://github.com/RageAgainstThePixel/com.rest.elevenlabs#authentication for details.");
            }

            Client = httpClient ?? new HttpClient();
            Client.DefaultRequestHeaders.Add("User-Agent", "com.rest.elevenlabs");
            Client.DefaultRequestHeaders.Add("xi-api-key", ElevenLabsAuthentication.ApiKey);

            JsonSerializationOptions = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            UserEndpoint = new UserEndpoint(this);
            VoicesEndpoint = new VoicesEndpoint(this);
            HistoryEndpoint = new HistoryEndpoint(this);
            TextToSpeechEndpoint = new TextToSpeechEndpoint(this);
            VoiceGenerationEndpoint = new VoiceGenerationEndpoint(this);
        }

        /// <summary>
        /// <see cref="HttpClient"/> to use when making calls to the API.
        /// </summary>
        internal HttpClient Client { get; }

        /// <summary>
        /// The <see cref="JsonSerializationOptions"/> to use when making calls to the API.
        /// </summary>
        internal JsonSerializerSettings JsonSerializationOptions { get; }

        /// <summary>
        /// The API authentication information to use for API calls
        /// </summary>
        public ElevenLabsAuthentication ElevenLabsAuthentication { get; }

        internal ElevenLabsClientSettings ElevenLabsClientSettings { get; }

        public UserEndpoint UserEndpoint { get; }

        public VoicesEndpoint VoicesEndpoint { get; }

        public HistoryEndpoint HistoryEndpoint { get; }

        public TextToSpeechEndpoint TextToSpeechEndpoint { get; }

        public VoiceGenerationEndpoint VoiceGenerationEndpoint { get; }
    }
}
