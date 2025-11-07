// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Models;
using ElevenLabs.Voices;
using Newtonsoft.Json;
using System;

namespace ElevenLabs.TextToSpeech
{
    public sealed record TextToSpeechSessionConfiguration
    {
        public TextToSpeechSessionConfiguration(
            VoiceClip voice,
            VoiceSettings voiceSettings = null,
            Model model = null,
            string languageCode = null,
            bool? enableLogging = null,
            bool? enableSSMLParsing = null,
            int? inactivityTimeout = null,
            bool? syncAlignment = null,
            bool? autoMode = null,
            TextNormalization? textNormalization = null,
            uint? seed = null)
        {
            Voice = voice ?? throw new ArgumentNullException(nameof(voice));
            VoiceSettings = voiceSettings;
            Model = string.IsNullOrWhiteSpace(model?.Id) ? ElevenLabs.Models.Model.MultiLingualSpeechToSpeechV2 : model.Id;
            LanguageCode = languageCode;
            EnableLogging = enableLogging;
            EnableSSMLParsing = enableSSMLParsing;
            InactivityTimeout = inactivityTimeout;
            SyncAlignment = syncAlignment;
            AutoMode = autoMode;
            TextNormalization = textNormalization;
            Seed = seed;
        }

        [JsonIgnore]
        public VoiceClip Voice { get; }

        [JsonIgnore]
        public VoiceSettings VoiceSettings { get; }

        [JsonProperty("model_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Model { get; }

        [JsonProperty("language_code", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string LanguageCode { get; }

        [JsonProperty("enabled_logging", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? EnableLogging { get; }

        [JsonProperty("enable_ssml_parsing", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? EnableSSMLParsing { get; }

        [JsonProperty("inactivity_timeout", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? InactivityTimeout { get; }

        [JsonProperty("sync_alignment", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? SyncAlignment { get; }

        [JsonProperty("auto_mode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? AutoMode { get; }

        [JsonProperty("apply_text_normalization", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TextNormalization? TextNormalization { get; }

        [JsonProperty("seed", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public uint? Seed { get; }
    }
}
