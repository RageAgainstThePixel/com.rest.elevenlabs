using System;
using UnityEngine;

namespace ElevenLabs.Models
{
    /// <summary>
    /// Represents timing information for a single character in the transcript
    /// </summary>
    [Serializable]
    public class PartialTranscribedAudioClip
    {
        private AudioClip audioClip;
        private TimestampedTranscriptCharacter[] timestampedTranscriptCharacters;

        public AudioClip AudioClip => audioClip;
        public TimestampedTranscriptCharacter[] TimestampedTranscriptCharacters => timestampedTranscriptCharacters;

        public PartialTranscribedAudioClip(AudioClip audioClip, TimestampedTranscriptCharacter[] timestampedTranscriptCharacters)
        {
            this.audioClip = audioClip;
            this.timestampedTranscriptCharacters = timestampedTranscriptCharacters;
        }
    }
}
