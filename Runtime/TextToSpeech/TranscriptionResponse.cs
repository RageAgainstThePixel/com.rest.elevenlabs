// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ElevenLabs.TextToSpeech
{
    [Preserve]
    internal sealed class TranscriptionResponse
    {
        [Preserve]
        [JsonConstructor]
        public TranscriptionResponse(
            [JsonProperty("audio_base64")] string audioBase64,
            [JsonProperty("alignment")] Alignment alignment)
        {
            AudioBase64 = audioBase64;
            Alignment = alignment;
        }

        [Preserve]
        [JsonProperty("audio_base64")]
        public string AudioBase64 { get; }

        [Preserve]
        [JsonProperty("alignment")]
        public Alignment Alignment { get; }
    }
}
