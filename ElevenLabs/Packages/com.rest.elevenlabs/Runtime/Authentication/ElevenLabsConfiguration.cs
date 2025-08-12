// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Voices;
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

        public string ApiKey
        {
            get => apiKey;
            internal set => apiKey = value;
        }

        [SerializeField]
        [Tooltip("Optional proxy domain to make requests though.")]
        private string proxyDomain;

        public string ProxyDomain => proxyDomain;

        [SerializeField]
        private Voice globalVoice;

        public Voice GlobalVoice => globalVoice;
    }
}
