// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using UnityEngine;

namespace ElevenLabs
{
    [Obsolete("Use " + nameof(ElevenLabsSettings))]
    public sealed class ElevenLabsClientSettings
    {
        public static implicit operator ElevenLabsSettings(ElevenLabsClientSettings clientSettings) => new ElevenLabsSettings(clientSettings);

        /// <summary>
        /// Creates a new instance of <see cref="ElevenLabsClientSettings"/> for use with ElevenLabs API.
        /// </summary>
        public ElevenLabsClientSettings()
        {
            Domain = ElevenLabsSettingsInfo.ElevenLabsDomain;
            ApiVersion = "v1";
            BaseRequest = $"/{ApiVersion}/";
            BaseRequestUrlFormat = $"https://{Domain}{BaseRequest}{{0}}";
        }

        /// <summary>
        /// Creates a new instance of <see cref="ElevenLabsClientSettings"/> for use with ElevenLabs API.
        /// </summary>
        /// <param name="domain">Base api domain.</param>
        /// <param name="apiVersion">The version of the ElevenLabs api you want to use.</param>
        public ElevenLabsClientSettings(string domain, string apiVersion = ElevenLabsSettingsInfo.DefaultApiVersion)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                domain = ElevenLabsSettingsInfo.ElevenLabsDomain;
            }

            if (!domain.Contains('.') &&
                !domain.Contains(':'))
            {
                throw new ArgumentException($"You're attempting to pass a \"resourceName\" parameter to \"{nameof(domain)}\". Please specify \"resourceName:\" for this parameter in constructor.");
            }

            if (string.IsNullOrWhiteSpace(apiVersion))
            {
                apiVersion = ElevenLabsSettingsInfo.DefaultApiVersion;
            }

            Domain = domain;
            ApiVersion = apiVersion;
            BaseRequest = $"/{ApiVersion}/";
            BaseRequestUrlFormat = $"https://{Domain}{BaseRequest}{{0}}";
        }

        public string Domain { get; }

        public string ApiVersion { get; }

        public string BaseRequest { get; }

        public string BaseRequestUrlFormat { get; }

        private static ElevenLabsClientSettings cachedDefault;

        public static ElevenLabsClientSettings Default
        {
            get
            {
                if (cachedDefault != null)
                {
                    return cachedDefault;
                }

                var config = Resources.LoadAll<ElevenLabsConfiguration>(string.Empty).FirstOrDefault(asset => asset != null);

                if (config != null)
                {
                    cachedDefault = new ElevenLabsClientSettings(
                        domain: config.ProxyDomain,
                        apiVersion: config.ApiVersion);
                }
                else
                {
                    cachedDefault = new ElevenLabsClientSettings();
                }

                return cachedDefault;
            }
        }
    }
}
