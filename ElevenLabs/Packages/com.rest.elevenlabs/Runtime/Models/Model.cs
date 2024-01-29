// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace ElevenLabs.Models
{
    [Preserve]
    public sealed class Model
    {
        [Preserve]
        public Model(string id)
        {
            Id = id;
        }

        [Preserve]
        [JsonConstructor]
        public Model(
            [JsonProperty("model_id")] string id,
            [JsonProperty("name")] string name,
            [JsonProperty("description")] string description,
            [JsonProperty("can_be_finetuned")] bool canBeFineTuned,
            [JsonProperty("can_do_text_to_speech")] bool canDoTextToSpeech,
            [JsonProperty("can_do_voice_conversion")] bool canDoVoiceConversion,
            [JsonProperty("token_cost_factor")] double tokenCostFactor,
            [JsonProperty("languages")] IReadOnlyList<Language> languages)
        {
            Id = id;
            Name = name;
            Description = description;
            CanBeFineTuned = canBeFineTuned;
            CanDoTextToSpeech = canDoTextToSpeech;
            CanDoVoiceConversion = canDoVoiceConversion;
            TokenCostFactor = tokenCostFactor;
            Languages = languages;
        }

        [Preserve]
        [JsonProperty("model_id")]
        public string Id { get; }

        [Preserve]
        [JsonProperty("name")]
        public string Name { get; }

        [Preserve]
        [JsonProperty("description")]
        public string Description { get; }

        [Preserve]
        [JsonProperty("can_be_finetuned")]
        public bool CanBeFineTuned { get; }

        [Preserve]
        [JsonProperty("can_do_text_to_speech")]
        public bool CanDoTextToSpeech { get; }

        [Preserve]
        [JsonProperty("can_do_voice_conversion")]
        public bool CanDoVoiceConversion { get; }

        [Preserve]
        [JsonProperty("token_cost_factor")]
        public double TokenCostFactor { get; }

        [Preserve]
        [JsonProperty("languages")]
        public IReadOnlyList<Language> Languages { get; }

        [Preserve]
        public static implicit operator string(Model model) => model.ToString();

        [Preserve]
        public override string ToString() => Id;

        #region Predefined Models

        [Preserve]
        [JsonIgnore]
        [Obsolete("Use EnglishV1")]
        public static Model MonoLingualV1 => EnglishV1;

        /// <summary>
        /// Use our standard English language model to generate speech in a variety of voices, styles and moods.
        /// </summary>
        [Preserve]
        [JsonIgnore]
        public static Model EnglishV1 { get; } = new("eleven_monolingual_v1");

        /// <summary>
        /// Speech to speech model suitable for scenarios where you need maximum control over the content and prosody of your generations.
        /// </summary>
        [Preserve]
        [JsonIgnore]
        public static Model EnglishV2 { get; } = new("eleven_english_sts_v2");

        /// <summary>
        /// Cutting-edge turbo model is ideally suited for tasks demanding extremely low latency.
        /// </summary>
        [Preserve]
        [JsonIgnore]
        public static Model EnglishTurboV2 { get; } = new("eleven_turbo_v2");

        /// <summary>
        /// Generate lifelike speech in multiple languages and create content that resonates with a broader audience.
        /// </summary>
        [Preserve]
        [JsonIgnore]
        public static Model MultiLingualV1 { get; } = new("eleven_multilingual_v1");

        /// <summary>
        /// State of the art multilingual speech synthesis model, able to generate life-like speech in 29 languages.
        /// </summary>
        [Preserve]
        [JsonIgnore]
        public static Model MultiLingualV2 { get; } = new("eleven_multilingual_v2");

        #endregion Predefined Models
    }
}
