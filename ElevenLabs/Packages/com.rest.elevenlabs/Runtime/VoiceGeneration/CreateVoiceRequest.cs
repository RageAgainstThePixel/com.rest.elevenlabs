// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace ElevenLabs.VoiceGeneration
{
    public sealed class CreateVoiceRequest
    {
        public CreateVoiceRequest(string voiceName, string generatedVoiceId = null)
        {
            VoiceName = voiceName;
            GeneratedVoiceId = generatedVoiceId;
        }

        [JsonProperty("voice_name")]
        public string VoiceName { get; }

        [JsonProperty("generated_voice_id")]
        public string GeneratedVoiceId { get; }
    }
}
