// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace ElevenLabs.Voices
{
    /// <summary>
    /// Access to voices created either by you or us.
    /// </summary>
    public class VoicesEndpoint : BaseEndPoint
    {
        public VoicesEndpoint(ElevenLabsClient api) : base(api) { }

        protected override string GetEndpoint()
            => $"{Api.BaseUrl}voices";

        /// <summary>
        /// Gets a list of all available voices for a user.
        /// </summary>
        public async Task GetVoicesAsync()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets the default settings for voices.
        /// </summary>
        public async Task GetDefaultVoiceSettingsAsync()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets the settings for a specific voice.
        /// </summary>
        /// <param name="voiceId"></param>
        public async Task GetVoiceSettingsAsync(string voiceId)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets metadata about a specific voice.
        /// </summary>
        /// <param name="voiceId"></param>
        public async Task GetVoiceAsync(string voiceId)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Delete a voice by its <see cref="voiceId"/>.
        /// </summary>
        /// <param name="voiceId"></param>
        /// <returns></returns>
        public async Task DeleteVoiceAsync(string voiceId)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Edit your settings for a specific voice.
        /// </summary>
        /// <param name="voiceId"></param>
        /// <returns></returns>
        public async Task EditVoiceSettingsAsync(string voiceId)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Add a new voice to your collection of voices in VoiceLab.
        /// </summary>
        public async Task AddVoiceAsync()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Edit a voice created by you.
        /// </summary>
        public async Task EditVoiceAsync()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Get the audio corresponding to a sample attached to a voice.
        /// </summary>
        /// <param name="voiceId"></param>
        /// <param name="sampleId"></param>
        public async Task GetVoiceSampleAsync(string voiceId, string sampleId)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Delete the audio corresponding to a sample attached to a voice.
        /// </summary>
        /// <param name="voiceId"></param>
        /// <param name="sampleId"></param>
        /// <returns></returns>
        public async Task DeleteVoiceSampleAsync(string voiceId, string sampleId)
        {
            await Task.CompletedTask;
        }
    }
}
