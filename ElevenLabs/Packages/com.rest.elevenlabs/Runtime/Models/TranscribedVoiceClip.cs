using System;

namespace ElevenLabs.Models
{
    /// <summary>
    /// Represents timing information for a single character in the transcript
    /// </summary>
    [Serializable]
    public class TranscribedVoiceClip
    {
        public readonly VoiceClip VoiceClip;
        public readonly TimestampedTranscriptCharacter[] timestampedTranscriptCharacters;

        public TranscribedVoiceClip(VoiceClip voiceClip, TimestampedTranscriptCharacter[] timestampedTranscriptCharacters)
        {
            this.VoiceClip = voiceClip;
            this.timestampedTranscriptCharacters = timestampedTranscriptCharacters;
        }
    }
}
