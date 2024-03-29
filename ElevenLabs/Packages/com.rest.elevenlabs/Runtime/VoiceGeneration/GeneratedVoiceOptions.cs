// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace ElevenLabs.VoiceGeneration
{
    [Preserve]
    public sealed class GeneratedVoiceOptions
    {
        [Preserve]
        [JsonConstructor]
        public GeneratedVoiceOptions(
            [JsonProperty("genders")] List<Gender> genders,
            [JsonProperty("accents")] List<Accent> accents,
            [JsonProperty("ages")] List<Age> ages,
            [JsonProperty("minimum_characters")] int minimumCharacters,
            [JsonProperty("maximum_characters")] int maximumCharacters,
            [JsonProperty("minimum_accent_strength")] double minimumAccentStrength,
            [JsonProperty("maximum_accent_strength")] double maximumAccentStrength)
        {
            Genders = genders;
            Accents = accents;
            Ages = ages;
            MinimumCharacters = minimumCharacters;
            MaximumCharacters = maximumCharacters;
            MinimumAccentStrength = minimumAccentStrength;
            MaximumAccentStrength = maximumAccentStrength;
        }

        [Preserve]
        [JsonProperty("genders")]
        public IReadOnlyList<Gender> Genders { get; }

        [Preserve]
        [JsonProperty("accents")]
        public IReadOnlyList<Accent> Accents { get; }

        [Preserve]
        [JsonProperty("ages")]
        public IReadOnlyList<Age> Ages { get; }

        [Preserve]
        [JsonProperty("minimum_characters")]
        public int MinimumCharacters { get; }

        [Preserve]
        [JsonProperty("maximum_characters")]
        public int MaximumCharacters { get; }

        [Preserve]
        [JsonProperty("minimum_accent_strength")]
        public double MinimumAccentStrength { get; }

        [Preserve]
        [JsonProperty("maximum_accent_strength")]
        public double MaximumAccentStrength { get; }
    }
}
