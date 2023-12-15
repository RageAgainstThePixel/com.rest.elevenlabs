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
        public TextToSpeechRequest(Voice voice, string text, Encoding encoding = null, VoiceSettings voiceSettings = null, OutputFormat outputFormat = OutputFormat.MP3_44100_128, int? optimizeStreamingLatency = null, Model model = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (text.Length > 5000)
            {
                throw new ArgumentOutOfRangeException(nameof(text), $"{nameof(text)} cannot exceed 5000 characters");
            }

            if (voice == null ||
                string.IsNullOrWhiteSpace(voice.Id))
            {
                throw new ArgumentNullException(nameof(voice));
            }

            if (encoding?.Equals(Encoding.UTF8) == false)
            {
                text = Encoding.UTF8.GetString(encoding.GetBytes(text));
            }

            Text = text;
            Model = model ?? Models.Model.MonoLingualV1;
            Voice = voice;
            VoiceSettings = voiceSettings ?? voice.Settings ?? throw new ArgumentNullException(nameof(voiceSettings));
            OutputFormat = outputFormat;
            OptimizeStreamingLatency = optimizeStreamingLatency;
        }

        [Preserve]
        [JsonProperty("text")]
        public string Text { get; }

        [Preserve]
        [JsonProperty("model_id")]
        public string Model { get; }

        [Preserve]
        [JsonIgnore]
        public Voice Voice { get; }

        [Preserve]
        [JsonProperty("voice_settings")]
        public VoiceSettings VoiceSettings { get; internal set; }

        [Preserve]
        [JsonIgnore]
        public OutputFormat OutputFormat { get; }

        [Preserve]
        [JsonIgnore]
        public int? OptimizeStreamingLatency { get; }
    }
}
