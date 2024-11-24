// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Voices;
using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace ElevenLabs
{
    [Preserve]
    [Serializable]
    public sealed class VoiceClip : GeneratedClip
    {
        [Preserve]
        internal VoiceClip(string id, string text, Voice voice, AudioClip audioClip, string cachedPath)
            : base(id, text, audioClip, cachedPath)
        {
            this.voice = voice;
        }

        [SerializeField]
        private Voice voice;

        [Preserve]
        public Voice Voice => voice;

        [Preserve]
        public TimestampedTranscriptCharacter[] TimestampedTranscriptCharacters { get; internal set; }
    }
}
