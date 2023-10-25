// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using ElevenLabs.Voices;
using UnityEngine;
using UnityEngine.Scripting;

namespace ElevenLabs
{
    public sealed class DownloadItem
    {
        [Preserve]
        internal DownloadItem(string id, string text, Voice voice, AudioClip audioClip, string cachedPath)
        {
            Id = id;
            Text = text;
            Voice = voice;
            TextHash = $"{id}{text}".GenerateGuid().ToString();
            AudioClip = audioClip;
            CachedPath = cachedPath;
        }

        public string Id { get; }

        public string Text { get; }

        public Voice Voice { get; }

        public string TextHash { get; }

        public AudioClip AudioClip { get; }

        public string CachedPath { get; }
    }
}
