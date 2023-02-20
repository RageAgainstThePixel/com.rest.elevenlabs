// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using ElevenLabs.History;
using ElevenLabs.TextToSpeech;
using ElevenLabs.User;
using ElevenLabs.Voices;

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
        /// <exception cref="AuthenticationException">Raised when authentication details are missing or invalid.</exception>
        public ElevenLabsClient(ElevenLabsAuthentication elevenLabsAuthentication = null)
        {
            ElevenLabsAuthentication = elevenLabsAuthentication ?? ElevenLabsAuthentication.Default;

            if (ElevenLabsAuthentication?.ApiKey is null)
            {
                throw new AuthenticationException("You must provide API authentication.  Please refer to https://github.com/RageAgainstThePixel/com.rest.elevenlabs#authentication for details.");
            }

            Client = new HttpClient();
            Client.DefaultRequestHeaders.Add("User-Agent", "com.openai.unity");
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ElevenLabsAuthentication.ApiKey);

            Version = 1;
            JsonSerializationOptions = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            UserEndpoint = new UserEndpoint(this);
            VoicesEndpoint = new VoicesEndpoint(this);
            HistoryEndpoint = new HistoryEndpoint(this);
            TextToSpeechEndpoint = new TextToSpeechEndpoint(this);
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

        private int version;

        /// <summary>
        /// Specifies which version of the API to use.
        /// </summary>
        public int Version
        {
            get => version;
            set
            {
                version = value;
                BaseUrl = $"https://api.openai.com/v{version}/";
            }
        }

        /// <summary>
        /// The base url to use when making calls to the API.
        /// </summary>
        internal string BaseUrl { get; private set; }

        public UserEndpoint UserEndpoint { get; }

        public VoicesEndpoint VoicesEndpoint { get; }

        public HistoryEndpoint HistoryEndpoint { get; }

        public TextToSpeechEndpoint TextToSpeechEndpoint { get; }
    }
}
