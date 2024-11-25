// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ElevenLabs
{
    /// <summary>
    /// Represents timing information for a single character in the transcript
    /// </summary>
    [Preserve]
    public class TimestampedTranscriptCharacter
    {
        [Preserve]
        [JsonConstructor]
        internal TimestampedTranscriptCharacter(string character, double startTime, double endTime)
        {
            Character = character;
            StartTime = startTime;
            EndTime = endTime;
        }

        /// <summary>
        /// The character being spoken
        /// </summary>
        [Preserve]
        [JsonProperty("character")]
        public string Character { get; }

        /// <summary>
        /// The time in seconds when this character starts being spoken
        /// </summary>
        [Preserve]
        [JsonProperty("character_start_times_seconds")]
        public double StartTime { get; }

        /// <summary>
        /// The time in seconds when this character finishes being spoken
        /// </summary>
        [Preserve]
        [JsonProperty("character_end_times_seconds")]
        public double EndTime { get; }
    }
}
