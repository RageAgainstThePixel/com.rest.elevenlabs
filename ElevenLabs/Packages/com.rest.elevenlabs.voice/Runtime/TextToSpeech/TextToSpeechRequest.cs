using ElevenLabs.Voices;
using Newtonsoft.Json;

namespace ElevenLabs.TextToSpeech
{
    public sealed class TextToSpeechRequest
    {
        [JsonConstructor]
        public TextToSpeechRequest(
            [JsonProperty("text")] string text,
            [JsonProperty("voice_settings")] VoiceSettings voiceSettings)
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
