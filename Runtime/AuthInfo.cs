// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace ElevenLabs
{
    [Serializable]
    internal class AuthInfo
    {
        public AuthInfo(string apiKey)
        {
            this.apiKey = apiKey;
        }

        [SerializeField]
        private string apiKey;

        public string ApiKey => apiKey;
    }
}
