// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace ElevenLabs
{
    [CreateAssetMenu(fileName = nameof(ElevenLabsConfigurationSettings), menuName = "ElevenLabs/" + nameof(ElevenLabsConfigurationSettings), order = 0)]
    internal sealed class ElevenLabsConfigurationSettings : ScriptableObject
    {
        [SerializeField]
        private AuthInfo authInfo = new AuthInfo(string.Empty);

        public string ApiKey => authInfo.ApiKey;
    }
}
