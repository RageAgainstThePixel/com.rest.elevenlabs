// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Voices;
using Newtonsoft.Json;

namespace ElevenLabs.TextToSpeech
{
    internal sealed class TextToSpeechRequest
    {
        public TextToSpeechRequest(string text, VoiceSettings voiceSettings)
        {
            Text = text;
            VoiceSettings = voiceSettings;
        }

        [JsonProperty("text")]
        public string Text { get; }

        [JsonProperty("voice_settings")]
        public VoiceSettings VoiceSettings { get; }
    }
}
