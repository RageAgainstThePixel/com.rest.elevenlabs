using System;

namespace ElevenLabs.Models
{
    /// <summary>
    /// Represents timing information for a single character in the transcript
    /// </summary>
    [Serializable]
    public class TimestampedTranscriptCharacter
    {
        /// <summary>
        /// The character being spoken
        /// </summary>
        public readonly string Character;

        /// <summary>
        /// The time in seconds when this character starts being spoken
        /// </summary>
        public readonly double StartTime;

        /// <summary>
        /// The time in seconds when this character finishes being spoken
        /// </summary>
        public readonly double EndTime;

        public TimestampedTranscriptCharacter(string character, double startTime, double endTime)
        {
            Character = character;
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}
