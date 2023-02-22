// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using ElevenLabs.Voices;
using UnityEngine;

namespace ElevenLabs.TextToSpeech
{
    /// <summary>
    /// Access to convert text to synthesized speech.
    /// </summary>
    public sealed class TextToSpeechEndpoint : BaseEndPoint
    {
        public TextToSpeechEndpoint(ElevenLabsClient api) : base(api) { }

        protected override string GetEndpoint()
            => $"{Api.BaseUrl}text-to-speech";

        /// <summary>
        /// Converts text into speech using a voice of your choice and returns audio.
        /// </summary>
        /// <param name="text">Text input to synthesize speech for.</param>
        /// <param name="voice"><see cref="Voice"/> to use.</param>
        /// <param name="voiceSettings">Optional, <see cref="VoiceSettings"/> that will override the default settings in <see cref="voice"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="AudioClip"/>.</returns>
        public async Task<AudioClip> TextToSpeechAsync(string text, Voice voice, VoiceSettings voiceSettings = null, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            return null;
        }

        /// <summary>
        /// Converts text into speech using a voice of your choice and returns audio as an audio stream.
        /// </summary>
        /// <param name="text">Text input to synthesize speech for.</param>
        /// <param name="voice"><see cref="Voice"/> to use.</param>
        /// <param name="voiceSettings">Optional, <see cref="VoiceSettings"/> that will override the default settings in <see cref="voice"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        public Task StreamTextToSpeechAsync(string text, Voice voice, VoiceSettings voiceSettings = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
