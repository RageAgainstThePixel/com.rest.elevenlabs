// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElevenLabs.Voices
{
    [Serializable]
    public sealed class Voice : IEquatable<Voice>
    {
        public Voice(string id)
        {
            Id = id;
        }

        [JsonConstructor]
        public Voice(
            [JsonProperty("voice_id")] string id,
            [JsonProperty("name")] string name,
            [JsonProperty("samples")] List<Sample> samples,
            [JsonProperty("category")] string category,
            [JsonProperty("labels")] Dictionary<string, string> labels,
            [JsonProperty("preview_url")] string previewUrl,
            [JsonProperty("available_for_tiers")] List<string> availableForTiers,
            [JsonProperty("settings")] VoiceSettings settings)
        {
            Id = id;
            Name = name;
            Samples = samples;
            Category = category;
            Labels = labels;
            PreviewUrl = previewUrl;
            AvailableForTiers = availableForTiers;
            Settings = settings;
        }

        [SerializeField]
        private string name;

        [JsonProperty("name")]
        public string Name
        {
            get => name;
            private set => name = value;
        }

        [SerializeField]
        private string id;

        [JsonProperty("voice_id")]
        public string Id
        {
            get => id;
            private set => id = value;
        }

        [JsonProperty("samples")]
        public IReadOnlyList<Sample> Samples { get; }

        [JsonProperty("category")]
        public string Category { get; }

        [JsonProperty("labels")]
        public IReadOnlyDictionary<string, string> Labels { get; }

        [JsonProperty("preview_url")]
        public string PreviewUrl { get; }

        [JsonProperty("available_for_tiers")]
        public IReadOnlyList<string> AvailableForTiers { get; }

        [JsonProperty("settings")]
        public VoiceSettings Settings { get; internal set; }

        public static implicit operator string(Voice voice)
            => voice?.ToString();

        public static implicit operator Voice(string id)
            => string.IsNullOrWhiteSpace(id) ? null : new Voice(id);

        public override string ToString() => Id;

        #region Premade Voices

        [JsonIgnore]
        public static Voice Adam { get; } = new Voice("pNInz6obpgDQGcFmaJgB") { Name = nameof(Adam) };

        [JsonIgnore]
        public static Voice Antoni { get; } = new Voice("ErXwobaYiN019PkySvjV") { Name = nameof(Antoni) };

        [JsonIgnore]
        public static Voice Arnold { get; } = new Voice("VR6AewLTigWG4xSOukaG") { Name = nameof(Arnold) };

        [JsonIgnore]
        public static Voice Bella { get; } = new Voice("EXAVITQu4vr4xnSDxMaL") { Name = nameof(Bella) };

        [JsonIgnore]
        public static Voice Domi { get; } = new Voice("AZnzlk1XvdvUeBnXmlld") { Name = nameof(Domi) };

        [JsonIgnore]
        public static Voice Elli { get; } = new Voice("MF3mGyEYCl7XYWbV9V6O") { Name = nameof(Elli) };

        [JsonIgnore]
        public static Voice Josh { get; } = new Voice("TxGEqnHWrfWFTfGW9XjX") { Name = nameof(Josh) };

        [JsonIgnore]
        public static Voice Rachel { get; } = new Voice("21m00Tcm4TlvDq8ikWAM") { Name = nameof(Rachel) };

        [JsonIgnore]
        public static Voice Sam { get; } = new Voice("yoZ06aMxZJJ28mfd3POQ") { Name = nameof(Sam) };

        #endregion Premade Voices

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
                   Equals(Settings, other.Settings);
        }

        public override bool Equals(object voice)
            => ReferenceEquals(this, voice) || voice is Voice other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(name, id, Samples, Category, Labels, PreviewUrl, AvailableForTiers, Settings);

        public static bool operator !=(Voice left, Voice right) => !(left == right);

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
