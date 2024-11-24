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
        public TextToSpeechRequest(
            Voice voice,
            string text,
            Encoding encoding = null,
            VoiceSettings voiceSettings = null,
            OutputFormat outputFormat = OutputFormat.MP3_44100_128,
            int? optimizeStreamingLatency = null,
            Model model = null,
            string previousText = null,
            string nextText = null,
            string[] previousRequestIds = null,
            string[] nextRequestIds = null,
            string languageCode = null)
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
            Model = model ?? Models.Model.MultiLingualV2;
            Voice = voice;
            VoiceSettings = voiceSettings ?? voice.Settings ?? throw new ArgumentNullException(nameof(voiceSettings));
            OutputFormat = outputFormat;
            OptimizeStreamingLatency = optimizeStreamingLatency;
            PreviousText = previousText;
            NextText = nextText;
            if (previousRequestIds?.Length > 3)
            {
                previousRequestIds = previousRequestIds[..3];
            }
            PreviousRequestIds = previousRequestIds;
            if (nextRequestIds?.Length > 3)
            {
                nextRequestIds = nextRequestIds[..3];
            }
            NextRequestIds = nextRequestIds;
            LanguageCode = languageCode;
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

        [Preserve]
        [JsonProperty("previous_text")]
        public string PreviousText { get; }

        [Preserve]
        [JsonProperty("next_text")]
        public string NextText { get; }

        /// <remarks>
        /// A maximum of three next or previous history item ids can be sent
        /// </remarks>
        [Preserve]
        [JsonProperty("previous_request_ids")]
        public string[] PreviousRequestIds { get; }

        /// <remarks>
        /// A maximum of three next or previous history item ids can be sent
        /// </remarks>
        [Preserve]
        [JsonProperty("next_request_ids")]
        public string[] NextRequestIds { get; }

        [Preserve]
        [JsonProperty("language_code")]
        public string LanguageCode { get; }
    }
}
