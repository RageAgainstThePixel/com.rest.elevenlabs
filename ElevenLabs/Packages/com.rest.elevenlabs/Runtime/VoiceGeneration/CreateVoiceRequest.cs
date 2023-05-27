// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ElevenLabs.VoiceGeneration
{
    [Preserve]
    public sealed class CreateVoiceRequest
    {
        [Preserve]
        [JsonConstructor]
        public CreateVoiceRequest(
            [JsonProperty("voice_name")] string voiceName,
            [JsonProperty("generated_voice_id")] string generatedVoiceId = null)
        {
            VoiceName = voiceName;
            GeneratedVoiceId = generatedVoiceId;
        }

        [Preserve]
        [JsonProperty("voice_name")]
        public string VoiceName { get; }

        [Preserve]
        [JsonProperty("generated_voice_id")]
        public string GeneratedVoiceId { get; }
    }
}
