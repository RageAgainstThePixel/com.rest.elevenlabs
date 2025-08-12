// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Utilities.WebRequestRest.Interfaces;

namespace ElevenLabs
{
    public sealed class ElevenLabsSettingsInfo : ISettingsInfo
    {
        internal const string Http = "http://";
        internal const string Https = "https://";
        internal const string ElevenLabsDomain = "api.elevenlabs.io";

        /// <summary>
        /// Creates a new instance of <see cref="ElevenLabsSettingsInfo"/> for use with ElevenLabs.
        /// </summary>
        public ElevenLabsSettingsInfo()
        {
            Domain = ElevenLabsDomain;
            BaseRequestUrlFormat = $"{Https}{Domain}/{{0}}/{{1}}";
        }

        /// <summary>
        /// Creates a new instance of <see cref="ElevenLabsSettingsInfo"/> for use with ElevenLabs.
        /// </summary>
        /// <param name="domain">Base api domain.</param>
        public ElevenLabsSettingsInfo(string domain)
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

            var protocol = Https;

            if (domain.StartsWith(Http))
            {
                protocol = Http;
                domain = domain.Replace(Http, string.Empty);
            }
            else if (domain.StartsWith(Https))
            {
                protocol = Https;
                domain = domain.Replace(Https, string.Empty);
            }

            Domain = $"{protocol}{domain}";
            BaseRequestUrlFormat = $"{Domain}/{{0}}/{{1}}";
        }

        public string Domain { get; }

        public string BaseRequestUrlFormat { get; }
    }
}
