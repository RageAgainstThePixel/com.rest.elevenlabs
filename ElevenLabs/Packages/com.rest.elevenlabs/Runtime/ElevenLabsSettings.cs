// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using UnityEngine;
using Utilities.WebRequestRest.Interfaces;

namespace ElevenLabs
{
    public sealed class ElevenLabsSettings : ISettings<ElevenLabsSettingsInfo>
    {
        /// <summary>
        /// Creates a new instance of <see cref="ElevenLabsSettings"/> with default <see cref="ElevenLabsSettingsInfo"/>.
        /// </summary>
        public ElevenLabsSettings()
        {
            Info = new ElevenLabsSettingsInfo();
            cachedDefault = new ElevenLabsSettings(Info);
        }

        /// <summary>
        /// Creates a new instance of <see cref="ElevenLabsSettings"/> with provided <see cref="configuration"/>.
        /// </summary>
        /// <param name="configuration"><see cref="ElevenLabsConfiguration"/>.</param>
        public ElevenLabsSettings(ElevenLabsConfiguration configuration)
        {
            if (configuration == null)
            {
                Debug.LogWarning($"You can speed this up by passing a {nameof(ElevenLabsConfiguration)} to the {nameof(ElevenLabsSettings)}.ctr");
                configuration = Resources.LoadAll<ElevenLabsConfiguration>(string.Empty).FirstOrDefault(asset => asset != null);
            }

            if (configuration == null)
            {
                throw new MissingReferenceException($"Failed to find a valid {nameof(ElevenLabsConfiguration)}!");
            }

            Info = new ElevenLabsSettingsInfo(configuration.ProxyDomain, configuration.ApiVersion);
            cachedDefault = new ElevenLabsSettings(Info);
        }

        /// <summary>
        /// Creates a new instance of <see cref="ElevenLabsSettings"/> with the provided <see cref="settingsInfo"/>.
        /// </summary>
        /// <param name="settingsInfo"><see cref="ElevenLabsSettingsInfo"/>.</param>
        public ElevenLabsSettings(ElevenLabsSettingsInfo settingsInfo)
        {
            Info = settingsInfo;
            cachedDefault = this;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ElevenLabsSettings"/>.
        /// </summary>
        /// <param name="domain">Base api domain.</param>
        /// <param name="apiVersion">The version of the ElevenLabs api you want to use.</param>
        public ElevenLabsSettings(string domain, string apiVersion = ElevenLabsSettingsInfo.DefaultApiVersion)
        {
            Info = new ElevenLabsSettingsInfo(domain, apiVersion);
            cachedDefault = this;
        }

        private static ElevenLabsSettings cachedDefault;

        public static ElevenLabsSettings Default
        {
            get => cachedDefault ?? new ElevenLabsSettings(configuration: null);
            internal set => cachedDefault = value;
        }

        public ElevenLabsSettingsInfo Info { get; }

        public string BaseRequestUrlFormat => Info.BaseRequestUrlFormat;
    }
}
