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
        [Obsolete("use new .ctr overload")]
        public TextToSpeechRequest(
            Voice voice,
            string text,
            Encoding encoding,
            VoiceSettings voiceSettings,
            OutputFormat outputFormat,
            int? optimizeStreamingLatency,
            Model model = null,
            string previousText = null,
            string nextText = null,
            string[] previousRequestIds = null,
            string[] nextRequestIds = null,
            string languageCode = null,
            CacheFormat cacheFormat = CacheFormat.Wav,
            bool withTimestamps = false)
          : this(voice, text, encoding, voiceSettings, outputFormat, model, cacheFormat, previousText, nextText, previousRequestIds, nextRequestIds, languageCode, withTimestamps)
        {
            OptimizeStreamingLatency = optimizeStreamingLatency;
        }

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
        /// Optional, <see cref="Model"/> to use. Defaults to <see cref="Model.FlashV2"/>.
        /// </param>
        /// <param name="outputFormat">
        /// Output format of the generated audio.<br/>
        /// Defaults to <see cref="OutputFormat.MP3_44100_128"/>
        /// </param>
        /// <param name="previousText">
        /// The text that came before the text of the current request.
        /// Can be used to improve the speech’s continuity when concatenating together multiple generations or
        /// to influence the speech’s continuity in the current generation.
        /// </param>
        /// <param name="nextText">
        /// The text that comes after the text of the current request.
        /// Can be used to improve the speech’s continuity when concatenating together multiple generations or
        /// to influence the speech’s continuity in the current generation.
        /// </param>
        /// <param name="previousRequestIds">
        /// A list of request_id of the samples that were generated before this generation.
        /// Can be used to improve the speech’s continuity when splitting up a large task into multiple requests.
        /// The results will be best when the same model is used across the generations. In case both previous_text and previous_request_ids is send,
        /// previous_text will be ignored. A maximum of 3 request_ids can be send.
        /// </param>
        /// <param name="nextRequestIds">
        /// A list of request_id of the samples that come after this generation.
        /// next_request_ids is especially useful for maintaining the speech’s continuity when regenerating a sample that has had some audio quality issues.
        /// For example, if you have generated 3 speech clips, and you want to improve clip 2,
        /// passing the request id of clip 3 as a next_request_id (and that of clip 1 as a previous_request_id)
        /// will help maintain natural flow in the combined speech.
        /// The results will be best when the same model is used across the generations.
        /// In case both next_text and next_request_ids is send, next_text will be ignored.
        /// A maximum of 3 request_ids can be send.
        /// </param>
        /// <param name="languageCode">
        /// Optional, Language code (ISO 639-1) used to enforce a language for the model. Currently only <see cref="Model.TurboV2_5"/> supports language enforcement.
        /// For other models, an error will be returned if language code is provided.
        /// </param>
        /// <param name="cacheFormat">
        /// The audio format to save the audio in.
        /// Defaults to <see cref="CacheFormat.Wav"/>
        /// </param>
        /// <param name="withTimestamps">
        /// Generate speech from text with precise character-level timing information for audio-text synchronization.
        /// </param>
        /// <param name="seed">
        /// If specified, our system will make a best effort to sample deterministically,
        /// such that repeated requests with the same seed and parameters should return the same result.
        /// Determinism is not guaranteed. Must be integer between 0 and 4294967295.
        /// </param>
        /// <param name="applyTextNormalization">
        /// This parameter controls text normalization with three modes: ‘auto’ (null), ‘on’ (true), and ‘off’ (false).
        /// When set to ‘null’, the system will automatically decide whether to apply text normalization (e.g., spelling out numbers).
        /// With ‘true’, text normalization will always be applied,
        /// while with ‘false’, it will be skipped.
        /// Cannot be turned on for ‘eleven_turbo_v2_5’ model.
        /// </param>
        [Preserve]
        public TextToSpeechRequest(
            Voice voice,
            string text,
            Encoding encoding = null,
            VoiceSettings voiceSettings = null,
            OutputFormat outputFormat = OutputFormat.MP3_44100_128,
            Model model = null,
            CacheFormat cacheFormat = CacheFormat.Wav,
            string previousText = null,
            string nextText = null,
            string[] previousRequestIds = null,
            string[] nextRequestIds = null,
            string languageCode = null,
            bool withTimestamps = false,
            int? seed = null,
            bool? applyTextNormalization = null)
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
            Model = model ?? Models.Model.FlashV2;
            Voice = string.IsNullOrWhiteSpace(voice) ? Voice.Adam : voice;
            VoiceSettings = voiceSettings ?? voice.Settings;
            OutputFormat = outputFormat;
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
            Seed = seed;

            if (applyTextNormalization.HasValue)
            {
                ApplyTextNormalization = applyTextNormalization.Value ? "on" : "off";
            }
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
        [Obsolete("Deprecated")]
        public int? OptimizeStreamingLatency { get; }

        [Preserve]
        [JsonProperty("previous_text")]
        public string PreviousText { get; }

        [Preserve]
        [JsonProperty("next_text")]
        public string NextText { get; }

        [Preserve]
        [JsonProperty("previous_request_ids")]
        public string[] PreviousRequestIds { get; }

        [Preserve]
        [JsonProperty("next_request_ids")]
        public string[] NextRequestIds { get; }

        [Preserve]
        [JsonProperty("language_code")]
        public string LanguageCode { get; }

        [Preserve]
        [JsonIgnore]
        public bool WithTimestamps { get; }

        [Preserve]
        [JsonProperty("seed")]
        public int? Seed { get; }

        [Preserve]
        [JsonProperty("apply_text_normalization", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ApplyTextNormalization { get; }
    }
}
