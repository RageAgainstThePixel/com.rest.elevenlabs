// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Collections.Generic;

namespace ElevenLabs.Voices
{
    public sealed class Voice
    {
        public static implicit operator string(Voice voice) => voice.Id;

        [JsonConstructor]
        public Voice(
            [JsonProperty("voice_id")] string id,
            [JsonProperty("name")] string name,
            [JsonProperty("samples")] List<Sample> samples,
            [JsonProperty("category")] string category,
            [JsonProperty("labels")] Labels labels,
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

        [JsonProperty("voice_id")]
        public string Id { get; }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("samples")]
        public IReadOnlyList<Sample> Samples { get; }

        [JsonProperty("category")]
        public string Category { get; }

        [JsonProperty("labels")]
        public Labels Labels { get; }

        [JsonProperty("preview_url")]
        public string PreviewUrl { get; }

        [JsonProperty("available_for_tiers")]
        public IReadOnlyList<string> AvailableForTiers { get; }

        [JsonProperty("settings")]
        public VoiceSettings Settings { get; internal set; }
    }
}
