// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.History;
using ElevenLabs.Models;
using ElevenLabs.TextToSpeech;
using ElevenLabs.User;
using ElevenLabs.VoiceGeneration;
using ElevenLabs.Voices;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Authentication;
using Utilities.WebRequestRest;

namespace ElevenLabs
{
    public sealed class ElevenLabsClient : BaseClient<ElevenLabsAuthentication, ElevenLabsSettings>
    {
        /// <summary>
        /// Creates a new client for the Eleven Labs API, handling auth and allowing for access to various API endpoints.
        /// </summary>
        /// <param name="elevenLabsAuthentication">The API authentication information to use for API calls,
        /// or <see langword="null"/> to attempt to use the <see cref="ElevenLabsAuthentication.Default"/>,
        /// potentially loading from environment vars or from a config file.</param>
        /// <param name="clientSettings">Optional, <see cref="ElevenLabsClientSettings"/> for specifying a proxy domain.</param>
        /// <exception cref="AuthenticationException">Raised when authentication details are missing or invalid.</exception>
        public ElevenLabsClient(ElevenLabsAuthentication elevenLabsAuthentication = null, ElevenLabsSettings clientSettings = null)
            : base(elevenLabsAuthentication ?? ElevenLabsAuthentication.Default, clientSettings ?? ElevenLabsSettings.Default)
        {
            JsonSerializationOptions = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            UserEndpoint = new UserEndpoint(this);
            VoicesEndpoint = new VoicesEndpoint(this);
            ModelsEndpoint = new ModelsEndpoint(this);
            HistoryEndpoint = new HistoryEndpoint(this);
            TextToSpeechEndpoint = new TextToSpeechEndpoint(this);
            VoiceGenerationEndpoint = new VoiceGenerationEndpoint(this);
        }
        protected override void SetupDefaultRequestHeaders()
        {
            var headers = new Dictionary<string, string>
            {
#if !UNITY_WEBGL
                { "User-Agent", "com.rest.elevenlabs" },
#endif
                { "xi-api-key", Authentication.Info.ApiKey }
            };

            DefaultRequestHeaders = headers;
        }

        protected override void ValidateAuthentication()
        {
            if (!HasValidAuthentication)
            {
                throw new AuthenticationException("You must provide API authentication.  Please refer to https://github.com/RageAgainstThePixel/com.rest.elevenlabs#authentication for details.");
            }
        }

        public override bool HasValidAuthentication => !string.IsNullOrWhiteSpace(Authentication?.Info?.ApiKey);

        /// <summary>
        /// The <see cref="JsonSerializationOptions"/> to use when making calls to the API.
        /// </summary>
        internal JsonSerializerSettings JsonSerializationOptions { get; }

        public UserEndpoint UserEndpoint { get; }

        public VoicesEndpoint VoicesEndpoint { get; }

        public ModelsEndpoint ModelsEndpoint { get; }

        public HistoryEndpoint HistoryEndpoint { get; }

        public TextToSpeechEndpoint TextToSpeechEndpoint { get; }

        public VoiceGenerationEndpoint VoiceGenerationEndpoint { get; }
    }
}
