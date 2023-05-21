// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;
using Utilities.WebRequestRest.Interfaces;

namespace ElevenLabs
{
    [Serializable]
    public sealed class ElevenLabsAuthInfo : IAuthInfo
    {
        public ElevenLabsAuthInfo(string apiKey)
        {
            this.apiKey = apiKey;
        }

        [SerializeField]
        private string apiKey;

        public string ApiKey => apiKey;
    }
}
