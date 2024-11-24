// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Voices;
using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace ElevenLabs
{
    /// <summary>
    /// Represents timing information for a single character in the transcript
    /// </summary>
    [Preserve]
    [Serializable]
    public class TranscribedVoiceClip : VoiceClip
    {
        [Preserve]
        internal TranscribedVoiceClip(TimestampedTranscriptCharacter[] timestampedTranscriptCharacters, string id, string text, Voice voice, AudioClip audioClip, string cachedPath)
            : base(id, text, voice, audioClip, cachedPath)
        {
            TimestampedTranscriptCharacters = timestampedTranscriptCharacters;
        }

        [Preserve]
        public TimestampedTranscriptCharacter[] TimestampedTranscriptCharacters { get; }
    }
}
