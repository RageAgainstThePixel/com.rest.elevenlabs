// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using ElevenLabs.Extensions;
using ElevenLabs.Voices;
using UnityEngine;
using UnityEngine.Scripting;

namespace ElevenLabs
{
    [Preserve]
    [Serializable]
    public sealed class VoiceClip : ISerializationCallbackReceiver
    {
        [Preserve]
        internal VoiceClip(string id, string text, Voice voice, AudioClip audioClip, string cachedPath)
        {
            this.id = id;
            this.text = text;
            this.voice = voice;
            TextHash = $"{id}{text}".GenerateGuid();
            textHash = TextHash.ToString();
            this.audioClip = audioClip;
            this.cachedPath = cachedPath;
        }

        [SerializeField]
        private string id;

        [Preserve]
        public string Id => id;

        [SerializeField]
        private string text;

        [Preserve]
        public string Text => text;

        [SerializeField]
        private Voice voice;

        [Preserve]
        public Voice Voice => voice;

        [SerializeField]
        private string textHash;

        [Preserve]
        public Guid TextHash { get; private set; }

        [SerializeField]
        private AudioClip audioClip;

        [Preserve]
        public AudioClip AudioClip => audioClip;

        [SerializeField]
        private string cachedPath;

        [Preserve]
        public string CachedPath => cachedPath;

        public void OnBeforeSerialize() => textHash = TextHash.ToString();

        public void OnAfterDeserialize() => TextHash = Guid.Parse(textHash);
    }
}
