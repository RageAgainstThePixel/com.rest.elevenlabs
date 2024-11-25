// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace ElevenLabs
{
    [Preserve]
    [Serializable]
    public class GeneratedClip : ISerializationCallbackReceiver
    {
        [Preserve]
        internal GeneratedClip(string id, string text, AudioClip audioClip, string cachedPath)
        {
            this.id = id;
            this.text = text;
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

        public static implicit operator AudioClip(GeneratedClip clip) => clip?.audioClip;
    }
}
