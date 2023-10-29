// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using ElevenLabs.Extensions;
using ElevenLabs.Voices;
using UnityEngine;
using UnityEngine.Scripting;

namespace ElevenLabs
{
    [Preserve]
    public sealed class VoiceClip
    {
        [Preserve]
        internal VoiceClip(string id, string text, Voice voice, AudioClip audioClip, string cachedPath)
        {
            Id = id;
            Text = text;
            Voice = voice;
            TextHash = $"{id}{text}".GenerateGuid();
            AudioClip = audioClip;
            CachedPath = cachedPath;
        }

        [Preserve]
        public string Id { get; }

        [Preserve]
        public string Text { get; }

        [Preserve]
        public Voice Voice { get; }

        [Preserve]
        public Guid TextHash { get; }

        [Preserve]
        public AudioClip AudioClip { get; }

        [Preserve]
        public string CachedPath { get; }
    }
}
