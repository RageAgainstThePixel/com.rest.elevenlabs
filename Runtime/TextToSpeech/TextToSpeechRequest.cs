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
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="voice">
        /// <see cref="Voice"/> to use.
        /// </param>
        /// <param name="text">
        /// Text input to synthesize speech for.
        /// </param>
        /// <param name="encoding"><see cref="Encoding"/> to use for <see cref="text"/>.</param>
        /// <param name="voiceSettings">
        /// Optional, <see cref="VoiceSettings"/> that will override the default settings in <see cref="Voice.Settings"/>.
        /// </param>
        /// <param name="model">
        /// Optional, <see cref="Model"/> to use. Defaults to <see cref="Model.TurboV2_5"/>.
        /// </param>
        /// <param name="outputFormat">
        /// Output format of the generated audio.<br/>
        /// Defaults to <see cref="OutputFormat.MP3_44100_128"/>
        /// </param>
        /// <param name="optimizeStreamingLatency">
        /// Optional, You can turn on latency optimizations at some cost of quality.
        /// The best possible final latency varies by model.<br/>
        /// Possible values:<br/>
        /// 0 - default mode (no latency optimizations)<br/>
        /// 1 - normal latency optimizations (about 50% of possible latency improvement of option 3)<br/>
        /// 2 - strong latency optimizations (about 75% of possible latency improvement of option 3)<br/>
        /// 3 - max latency optimizations<br/>
        /// 4 - max latency optimizations, but also with text normalizer turned off for even more latency savings
        /// (best latency, but can mispronounce e.g. numbers and dates).
        /// </param>
        /// <param name="previousText"></param>
        /// <param name="nextText"></param>
        /// <param name="previousRequestIds"></param>
        /// <param name="nextRequestIds"></param>
        /// <param name="languageCode">
        /// Optional, Language code (ISO 639-1) used to enforce a language for the model. Currently only <see cref="Model.TurboV2_5"/> supports language enforcement.
        /// For other models, an error will be returned if language code is provided.
        /// </param>
        /// <param name="cacheFormat"></param>
        /// <param name="withTimestamps"></param>
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
            string languageCode = null,
            CacheFormat cacheFormat = CacheFormat.Wav,
            bool withTimestamps = false)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(nameof(text));
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
            Model = model ?? Models.Model.TurboV2_5;
            Voice = string.IsNullOrWhiteSpace(voice) ? Voice.Adam : voice;
            VoiceSettings = voiceSettings ?? voice.Settings;
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
            CacheFormat = cacheFormat;
            WithTimestamps = withTimestamps;
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
        public OutputFormat OutputFormat { get; internal set; }

        [Preserve]
        [JsonIgnore]
        public CacheFormat CacheFormat { get; internal set; }

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

        [Preserve]
        [JsonIgnore]
        public bool WithTimestamps { get; }
    }
}
