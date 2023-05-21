// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using UnityEngine;
using Utilities.WebRequestRest.Interfaces;

namespace ElevenLabs
{
    public sealed class ElevenLabsSettings : ISettings<ElevenLabsSettingsInfo>
    {
        /// <summary>
        /// Creates a new instance of <see cref="ElevenLabsSettings"/> for use with ElevenLabs.
        /// </summary>
        public ElevenLabsSettings()
        {
            if (cachedDefault != null) { return; }

            var config = Resources.LoadAll<ElevenLabsConfiguration>(string.Empty)
                .FirstOrDefault(asset => asset != null);

            if (config != null)
            {
                Default = new ElevenLabsSettings(new ElevenLabsSettingsInfo(
                    config.ProxyDomain,
                    config.ApiVersion));
            }
            else
            {
                Default = new ElevenLabsSettings(new ElevenLabsSettingsInfo());
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="ElevenLabsSettings"/> with the provided <see cref="ElevenLabsSettingsInfo"/>.
        /// </summary>
        /// <param name="settingsInfo"></param>
        public ElevenLabsSettings(ElevenLabsSettingsInfo settingsInfo)
            => this.settingsInfo = settingsInfo;

        /// <summary>
        /// Creates a new instance of <see cref="ElevenLabsSettings"/> for use with ElevenLabs.
        /// </summary>
        /// <param name="domain">Base api domain.</param>
        /// <param name="apiVersion">The version of the ElevenLabs api you want to use.</param>
        public ElevenLabsSettings(string domain, string apiVersion = ElevenLabsSettingsInfo.DefaultApiVersion)
            => settingsInfo = new ElevenLabsSettingsInfo(domain, apiVersion);

        [Obsolete("Obsolete")]
        internal ElevenLabsSettings(ElevenLabsClientSettings clientSettings) => settingsInfo = new ElevenLabsSettingsInfo(clientSettings);

        private static ElevenLabsSettings cachedDefault;

        public static ElevenLabsSettings Default
        {
            get => cachedDefault ?? new ElevenLabsSettings();
            internal set => cachedDefault = value;
        }

        private readonly ElevenLabsSettingsInfo settingsInfo;

        public ElevenLabsSettingsInfo Info => settingsInfo ?? Default.Info;

        public string BaseRequestUrlFormat => Info.BaseRequestUrlFormat;
    }
}
