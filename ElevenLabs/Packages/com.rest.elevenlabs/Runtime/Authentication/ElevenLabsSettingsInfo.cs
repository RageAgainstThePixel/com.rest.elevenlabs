// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Utilities.WebRequestRest.Interfaces;

namespace ElevenLabs
{
    public sealed class ElevenLabsSettingsInfo : ISettingsInfo
    {
        internal const string Https = "https://";
        internal const string ElevenLabsDomain = "api.elevenlabs.io";
        internal const string DefaultApiVersion = "v1";

        /// <summary>
        /// Creates a new instance of <see cref="ElevenLabsSettingsInfo"/> for use with ElevenLabs.
        /// </summary>
        public ElevenLabsSettingsInfo()
        {
            Domain = ElevenLabsDomain;
            ApiVersion = DefaultApiVersion;
            BaseRequest = $"/{ApiVersion}/";
            BaseRequestUrlFormat = $"{Https}{Domain}{BaseRequest}{{0}}";
        }

        /// <summary>
        /// Creates a new instance of <see cref="ElevenLabsSettingsInfo"/> for use with ElevenLabs.
        /// </summary>
        /// <param name="domain">Base api domain.</param>
        /// <param name="apiVersion">The version of the ElevenLabs api you want to use.</param>
        public ElevenLabsSettingsInfo(string domain, string apiVersion = DefaultApiVersion)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                domain = ElevenLabsDomain;
            }

            if (!domain.Contains('.') &&
                !domain.Contains(':'))
            {
                throw new ArgumentException($"Invalid parameter \"{nameof(domain)}\".");
            }

            if (string.IsNullOrWhiteSpace(apiVersion))
            {
                apiVersion = DefaultApiVersion;
            }

            Domain = domain.Contains("http") ? domain : $"{Https}{domain}";
            ApiVersion = apiVersion;
            BaseRequest = $"/{ApiVersion}/";
            BaseRequestUrlFormat = $"{Domain}{BaseRequest}{{0}}";
            BaseWebSocketUrlFormat = $"{WebSocketDomain}{BaseRequest}{{0}}";
        }

        public string Domain { get; }

        public string ApiVersion { get; }

        public string BaseRequest { get; }

        public string BaseRequestUrlFormat { get; }
    }
}
