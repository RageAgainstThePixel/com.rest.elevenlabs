// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace ElevenLabs
{
    [CreateAssetMenu(fileName = nameof(ElevenLabsConfigurationSettings), menuName = nameof(ElevenLabs) + "/" + nameof(ElevenLabsConfigurationSettings), order = 0)]
    internal sealed class ElevenLabsConfigurationSettings : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The xi api key.")]
        internal string apiKey;

        public string ApiKey => apiKey;

        [SerializeField]
        [Tooltip("Optional proxy domain to make requests though.")]
        private string proxyDomain;

        public string ProxyDomain => proxyDomain;

        [SerializeField]
        [Tooltip("The api version, Defaults to v1")]
        private string apiVersion = ElevenLabsClientSettings.DefaultApiVersion;

        public string ApiVersion => apiVersion;
    }
}
