// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Models;
using ElevenLabs.Voices;
using Newtonsoft.Json;
using System;
using System.Text;
using UnityEngine.Scripting;

namespace ElevenLabs.TextToSpeech
{
    [Preserve]
    public sealed class TextToSpeechRequest
    {
        [Preserve]
        [JsonConstructor]
        public TextToSpeechRequest(
            [JsonProperty("text")] string text,
            [JsonProperty("model_id")] Model model = null,
            [JsonProperty("voice_settings")] VoiceSettings voiceSettings = null)
            : this(text, model, voiceSettings, Encoding.UTF8)
        {
        }

        [Preserve]
        public TextToSpeechRequest(string text, Model model = null, VoiceSettings voiceSettings = null, Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (encoding?.Equals(Encoding.UTF8) == false)
            {
                text = Encoding.UTF8.GetString(encoding.GetBytes(text));
            }

            Text = text;
            Model = model ?? Models.Model.MonoLingualV1;
            VoiceSettings = voiceSettings ?? throw new ArgumentNullException(nameof(voiceSettings));
        }

        [Preserve]
        [JsonProperty("text")]
        public string Text { get; }

        [Preserve]
        [JsonProperty("model_id")]
        public string Model { get; }

        [Preserve]
        [JsonProperty("voice_settings")]
        public VoiceSettings VoiceSettings { get; internal set; }
    }
}
