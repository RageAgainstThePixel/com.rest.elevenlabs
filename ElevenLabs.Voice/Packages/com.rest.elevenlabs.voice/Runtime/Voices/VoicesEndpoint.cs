// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.WebRequestRest;

namespace ElevenLabs.Voices
{
    /// <summary>
    /// Access to voices created either by you or us.
    /// </summary>
    public sealed class VoicesEndpoint : BaseEndPoint
    {
        private class VoiceList
        {
            [JsonConstructor]
            public VoiceList([JsonProperty("voices")] List<Voice> voices)
            {
                Voices = voices;
            }

            [JsonProperty("voices")]
            public IReadOnlyList<Voice> Voices { get; }
        }

        public VoicesEndpoint(ElevenLabsClient api) : base(api) { }

        protected override string GetEndpoint()
            => $"{Api.BaseUrl}voices";

        /// <summary>
        /// Gets a list of all available voices for a user.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<Voice>> GetVoicesAsync(CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync(GetEndpoint(), cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            var voices = JsonConvert.DeserializeObject<VoiceList>(responseAsString, Api.JsonSerializationOptions).Voices;
            var voiceSettingsTasks = new List<Task>();

            foreach (var voice in voices)
            {
                voiceSettingsTasks.Add(Task.Run(LocalGetVoiceSettings, cancellationToken));

                async Task LocalGetVoiceSettings()
                {
                    voice.Settings = await GetVoiceSettingsAsync(voice, cancellationToken);
                }
            }

            await Task.WhenAll(voiceSettingsTasks);
            return voices.ToList();
        }

        /// <summary>
        /// Gets the default settings for voices.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<VoiceSettings> GetDefaultVoiceSettingsAsync(CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync($"{GetEndpoint()}/settings/default", cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<VoiceSettings>(responseAsString, Api.JsonSerializationOptions);
        }

        /// <summary>
        /// Gets the settings for a specific voice.
        /// </summary>
        /// <param name="voiceId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<VoiceSettings> GetVoiceSettingsAsync(string voiceId, CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync($"{GetEndpoint()}/{voiceId}/settings", cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<VoiceSettings>(responseAsString, Api.JsonSerializationOptions);
        }

        /// <summary>
        /// Gets metadata about a specific voice.
        /// </summary>
        /// <param name="voiceId"></param>
        /// <param name="withSettings"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Voice> GetVoiceAsync(string voiceId, bool withSettings = true, CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync($"{GetEndpoint()}/{voiceId}?with_settings={withSettings}", cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Voice>(responseAsString, Api.JsonSerializationOptions);
        }

        /// <summary>
        /// Edit your settings for a specific voice.
        /// </summary>
        /// <param name="voiceId"></param>
        /// <param name="voiceSettings"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> EditVoiceSettingsAsync(string voiceId, VoiceSettings voiceSettings, CancellationToken cancellationToken = default)
        {
            var payload = JsonConvert.SerializeObject(voiceSettings).ToJsonStringContent();
            var response = await Api.Client.PostAsync($"{GetEndpoint()}/{voiceId}/settings/edit", payload, cancellationToken);
            await response.ReadAsStringAsync();
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Add a new voice to your collection of voices in VoiceLab.
        /// </summary>
        public Task AddVoiceAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Edit a voice created by you.
        /// </summary>
        public Task EditVoiceAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete a voice by its <see cref="Voice.VoiceId"/>.
        /// </summary>
        /// <param name="voiceId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> DeleteVoiceAsync(string voiceId, CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.DeleteAsync($"{GetEndpoint()}/{voiceId}", cancellationToken);
            await response.ReadAsStringAsync(true);
            return response.IsSuccessStatusCode;
        }

        #region Samples

        /// <summary>
        /// Get the audio corresponding to a sample attached to a voice.
        /// </summary>
        /// <param name="voiceId"></param>
        /// <param name="sampleId"></param>
        /// <param name="cancellationToken"></param>
        public async Task<AudioClip> GetVoiceSampleAsync(string voiceId, string sampleId, CancellationToken cancellationToken = default)
        {
            var headers = Api.Client.DefaultRequestHeaders.ToDictionary(pair => pair.Key, pair => string.Join(" ", pair.Value));
            return await Rest.DownloadAudioClipAsync($"{GetEndpoint()}/{voiceId}/samples/{sampleId}/audio", AudioType.UNKNOWN, headers: headers, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Delete the audio corresponding to a sample attached to a voice.
        /// </summary>
        /// <param name="voiceId"></param>
        /// <param name="sampleId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> DeleteVoiceSampleAsync(string voiceId, string sampleId, CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.DeleteAsync($"{GetEndpoint()}/{voiceId}/samples/{sampleId}", cancellationToken);
            await response.ReadAsStringAsync(true);
            return response.IsSuccessStatusCode;
        }

        #endregion Samples
    }
}
