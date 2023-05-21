// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using Utilities.WebRequestRest.Interfaces;

namespace ElevenLabs
{
    [CreateAssetMenu(fileName = nameof(ElevenLabsConfiguration), menuName = nameof(ElevenLabs) + "/" + nameof(ElevenLabsConfiguration), order = 0)]
    public sealed class ElevenLabsConfiguration : ScriptableObject, IConfiguration
    {
        [SerializeField]
        [Tooltip("The xi api key.")]
        private string apiKey;

        public string ApiKey => apiKey;

        [SerializeField]
        [Tooltip("Optional proxy domain to make requests though.")]
        private string proxyDomain;

        public string ProxyDomain => proxyDomain;

        [SerializeField]
        [Tooltip("The api version, Defaults to v1")]
        private string apiVersion = ElevenLabsSettingsInfo.DefaultApiVersion;

        public string ApiVersion => apiVersion;
    }
}
