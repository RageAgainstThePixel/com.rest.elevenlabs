// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ElevenLabs.TextToSpeech
{
    [Preserve]
    internal sealed class Alignment
    {
        [Preserve]
        [JsonConstructor]
        internal Alignment(
            [JsonProperty("characters")] string[] characters,
            [JsonProperty("character_start_times_seconds")] double[] startTimes,
            [JsonProperty("character_end_times_seconds")] double[] endTimes)
        {
            Characters = characters;
            StartTimes = startTimes;
            EndTimes = endTimes;
        }

        [Preserve]
        [JsonProperty("characters")]
        public string[] Characters { get; }

        [Preserve]
        [JsonProperty("character_start_times_seconds")]
        public double[] StartTimes { get; }

        [Preserve]
        [JsonProperty("character_end_times_seconds")]
        public double[] EndTimes { get; }

        [Preserve]
        public static implicit operator TimestampedTranscriptCharacter[](Alignment alignment)
        {
            if (alignment == null) { return null; }
            var characters = alignment.Characters;
            var startTimes = alignment.StartTimes;
            var endTimes = alignment.EndTimes;
            var timestampedTranscriptCharacters = new TimestampedTranscriptCharacter[characters.Length];

            for (var i = 0; i < characters.Length; i++)
            {
                timestampedTranscriptCharacters[i] = new TimestampedTranscriptCharacter(characters[i], startTimes[i], endTimes[i]);
            }

            return timestampedTranscriptCharacters;
        }
    }
}
