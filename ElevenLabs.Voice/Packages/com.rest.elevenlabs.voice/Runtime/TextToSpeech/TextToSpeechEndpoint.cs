// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;

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
        /// <param name="voiceId"></param>
        public async Task TextToSpeechAsync(string voiceId)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Converts text into speech using a voice of your choice and returns audio as an audio stream.
        /// </summary>
        /// <param name="voiceId"></param>
        public async Task TextToSpeechStreamAsync(string voiceId)
        {
            await Task.CompletedTask;
        }
    }
}
