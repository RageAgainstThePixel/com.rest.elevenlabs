// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Models;
using ElevenLabs.Voices;
using Newtonsoft.Json;

namespace ElevenLabs.TextToSpeech
{
    internal sealed class TextToSpeechRequest
    {
        public TextToSpeechRequest(string text, Model model, VoiceSettings voiceSettings)
        {
            Text = text;
            Model = model;
            VoiceSettings = voiceSettings;
        }

        [JsonProperty("text")]
        public string Text { get; }

        [JsonProperty("model_id")]
        public string Model { get; }

        [JsonProperty("voice_settings")]
        public VoiceSettings VoiceSettings { get; }
    }
}
