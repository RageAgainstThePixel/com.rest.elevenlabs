// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace ElevenLabs.Voices
{
    [Preserve]
    [Serializable]
    public sealed class Voice : IEquatable<Voice>
    {
        [Preserve]
        public Voice(string id, string name)
        {
            Id = id;
            Name = name;
        }

        [Preserve]
        [JsonConstructor]
        public Voice(
            [JsonProperty("voice_id")] string id,
            [JsonProperty("name")] string name,
            [JsonProperty("samples")] List<Sample> samples,
            [JsonProperty("category")] string category,
            [JsonProperty("labels")] Dictionary<string, string> labels,
            [JsonProperty("preview_url")] string previewUrl,
            [JsonProperty("available_for_tiers")] List<string> availableForTiers,
            [JsonProperty("high_quality_base_model_ids")] List<string> highQualityBaseModelIds,
            [JsonProperty("settings")] VoiceSettings settings)
        {
            Id = id;
            Name = name;
            Samples = samples;
            Category = category;
            Labels = labels;
            PreviewUrl = previewUrl;
            AvailableForTiers = availableForTiers;
            HighQualityBaseModelIds = highQualityBaseModelIds;
            Settings = settings;
        }

        [SerializeField]
        private string name;

        [Preserve]
        [JsonProperty("name")]
        public string Name
        {
            get => name;
            private set => name = value;
        }

        [SerializeField]
        private string id;

        [Preserve]
        [JsonProperty("voice_id")]
        public string Id
        {
            get => id;
            private set => id = value;
        }

        [Preserve]
        [JsonProperty("samples")]
        public IReadOnlyList<Sample> Samples { get; }

        [Preserve]
        [JsonProperty("category")]
        public string Category { get; }

        [Preserve]
        [JsonProperty("labels")]
        public IReadOnlyDictionary<string, string> Labels { get; }

        [Preserve]
        [JsonProperty("preview_url")]
        public string PreviewUrl { get; }

        [Preserve]
        [JsonProperty("available_for_tiers")]
        public IReadOnlyList<string> AvailableForTiers { get; }

        [Preserve]
        [JsonProperty("high_quality_base_model_ids")]
        public IReadOnlyList<string> HighQualityBaseModelIds { get; }

        [Preserve]
        [JsonProperty("settings")]
        public VoiceSettings Settings { get; internal set; }

        [Preserve]
        public static implicit operator string(Voice voice) => voice.ToString();

        [Preserve]
        public override string ToString() => Id;

        #region Premade Voices

        [Preserve]
        [JsonIgnore]
        public static Voice Adam { get; } = new("pNInz6obpgDQGcFmaJgB", nameof(Adam));

        [Preserve]
        [JsonIgnore]
        public static Voice Antoni { get; } = new("ErXwobaYiN019PkySvjV", nameof(Antoni));

        [Preserve]
        [JsonIgnore]
        public static Voice Arnold { get; } = new("VR6AewLTigWG4xSOukaG", nameof(Arnold));

        [Preserve]
        [JsonIgnore]
        public static Voice Bella { get; } = new("EXAVITQu4vr4xnSDxMaL", nameof(Bella));

        [Preserve]
        [JsonIgnore]
        public static Voice Domi { get; } = new("AZnzlk1XvdvUeBnXmlld", nameof(Domi));

        [Preserve]
        [JsonIgnore]
        public static Voice Elli { get; } = new("MF3mGyEYCl7XYWbV9V6O", nameof(Elli));

        [Preserve]
        [JsonIgnore]
        public static Voice Josh { get; } = new("TxGEqnHWrfWFTfGW9XjX", nameof(Josh));

        [Preserve]
        [JsonIgnore]
        public static Voice Rachel { get; } = new("21m00Tcm4TlvDq8ikWAM", nameof(Rachel));

        [Preserve]
        [JsonIgnore]
        public static Voice Sam { get; } = new("yoZ06aMxZJJ28mfd3POQ", nameof(Sam));

        #endregion Premade Voices

        [Preserve]
        public bool Equals(Voice other)
        {
            if (ReferenceEquals(null, other))
            {
                return string.IsNullOrWhiteSpace(id);
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return name == other.name &&
                   id == other.id &&
                   Equals(Samples, other.Samples) &&
                   Category == other.Category &&
                   Equals(Labels, other.Labels) &&
                   PreviewUrl == other.PreviewUrl &&
                   Equals(AvailableForTiers, other.AvailableForTiers) &&
                   Equals(HighQualityBaseModelIds, other.HighQualityBaseModelIds) &&
                   Equals(Settings, other.Settings);
        }

        [Preserve]
        public override bool Equals(object voice)
            => ReferenceEquals(this, voice) || voice is Voice other && Equals(other);

        [Preserve]
        public override int GetHashCode()
            => HashCode.Combine(name, id, Samples, Category, Labels, PreviewUrl, AvailableForTiers, Settings);

        [Preserve]
        public static bool operator !=(Voice left, Voice right) => !(left == right);

        [Preserve]
        public static bool operator ==(Voice left, Voice right)
        {
            if (left is null && right is null)
            {
                return true;
            }

            if (left is null)
            {
                return string.IsNullOrWhiteSpace(right.id);
            }

            if (right is null)
            {
                return string.IsNullOrWhiteSpace(left.id);
            }

            return left.Equals(right);
        }
    }
}
