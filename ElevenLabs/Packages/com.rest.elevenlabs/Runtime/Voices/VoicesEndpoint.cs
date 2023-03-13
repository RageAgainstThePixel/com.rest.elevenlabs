// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

        private class VoiceResponse
        {
            [JsonConstructor]
            public VoiceResponse([JsonProperty("voice_id")] string voiceId)
            {
                VoiceId = voiceId;
            }

            [JsonProperty("voice_id")]
            public string VoiceId { get; }
        }

        public VoicesEndpoint(ElevenLabsClient api) : base(api) { }

        protected override string GetEndpoint()
            => $"{Api.BaseUrl}voices";

        /// <summary>
        /// Gets a list of all available voices for a user.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="IReadOnlyList{T}"/> of <see cref="Voice"/>s.</returns>
        public async Task<IReadOnlyList<Voice>> GetAllVoicesAsync(CancellationToken cancellationToken = default)
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
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceSettings"/>.</returns>
        public async Task<VoiceSettings> GetDefaultVoiceSettingsAsync(CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync($"{GetEndpoint()}/settings/default", cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<VoiceSettings>(responseAsString, Api.JsonSerializationOptions);
        }

        /// <summary>
        /// Gets the settings for a specific voice.
        /// </summary>
        /// <param name="voiceId">The id of the <see cref="Voice"/> to get <see cref="VoiceSettings"/> for.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceSettings"/>.</returns>
        public async Task<VoiceSettings> GetVoiceSettingsAsync(string voiceId, CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync($"{GetEndpoint()}/{voiceId}/settings", cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<VoiceSettings>(responseAsString, Api.JsonSerializationOptions);
        }

        /// <summary>
        /// Gets metadata about a specific voice.
        /// </summary>
        /// <param name="voiceId">The id of the <see cref="Voice"/> to get.</param>
        /// <param name="withSettings">Should the response include the <see cref="VoiceSettings"/>?</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Voice"/>.</returns>
        public async Task<Voice> GetVoiceAsync(string voiceId, bool withSettings = true, CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync($"{GetEndpoint()}/{voiceId}?with_settings={withSettings}", cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Voice>(responseAsString, Api.JsonSerializationOptions);
        }

        /// <summary>
        /// Edit your settings for a specific voice.
        /// </summary>
        /// <param name="voiceId">Id of the voice settings to edit.</param>
        /// <param name="voiceSettings"><see cref="VoiceSettings"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if voice settings was successfully edited.</returns>
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
        /// <param name="name">Name of the voice you want to add.</param>
        /// <param name="samplePaths">Collection of file paths to use as samples for the new voice.</param>
        /// <param name="labels">Optional, labels for the new voice.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        public async Task<Voice> AddVoiceAsync(string name, IEnumerable<string> samplePaths = null, IReadOnlyDictionary<string, string> labels = null, CancellationToken cancellationToken = default)
        {
            var form = new MultipartFormDataContent();

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            form.Add(new StringContent(name), "name");

            if (samplePaths != null)
            {
                samplePaths = samplePaths.ToList();

                if (samplePaths.Any())
                {
                    foreach (var sample in samplePaths)
                    {
                        if (string.IsNullOrWhiteSpace(sample))
                        {
                            throw new ArgumentNullException(nameof(sample));
                        }

                        var fileStream = File.OpenRead(sample);
                        var stream = new MemoryStream();
                        await fileStream.CopyToAsync(stream, cancellationToken);
                        form.Add(new ByteArrayContent(stream.ToArray()), "files", Path.GetFileName(sample));
                        await fileStream.DisposeAsync();
                        await stream.DisposeAsync();
                    }
                }
            }

            if (labels != null)
            {
                form.Add(new StringContent(JsonConvert.SerializeObject(labels)), "labels");
            }

            var response = await Api.Client.PostAsync($"{GetEndpoint()}/add", form, cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            var voiceResponse = JsonConvert.DeserializeObject<VoiceResponse>(responseAsString, Api.JsonSerializationOptions);
            var voice = await GetVoiceAsync(voiceResponse.VoiceId, cancellationToken: cancellationToken);
            return voice;
        }

        /// <summary>
        /// Edit a voice created by you.
        /// </summary>
        /// <param name="voice">The <see cref="Voice"/> to edit.</param>
        /// <param name="samplePaths">The full string paths of the <see cref="Sample"/>s to upload.</param>
        /// <param name="labels">The labels to set on the <see cref="Voice"/> description.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if voice was successfully edited.</returns>
        public async Task<bool> EditVoiceAsync(Voice voice, IEnumerable<string> samplePaths = null, IReadOnlyDictionary<string, string> labels = null, CancellationToken cancellationToken = default)
        {
            var form = new MultipartFormDataContent();

            if (voice == null)
            {
                throw new ArgumentNullException(nameof(voice));
            }

            form.Add(new StringContent(voice.Name), "name");

            if (samplePaths != null)
            {
                samplePaths = samplePaths.ToList();

                if (samplePaths.Any())
                {
                    foreach (var sample in samplePaths)
                    {
                        if (string.IsNullOrWhiteSpace(sample))
                        {
                            throw new ArgumentNullException(nameof(sample));
                        }

                        var fileStream = File.OpenRead(sample);
                        var stream = new MemoryStream();
                        await fileStream.CopyToAsync(stream, cancellationToken);
                        form.Add(new ByteArrayContent(stream.ToArray()), "files", Path.GetFileName(sample));
                        await fileStream.DisposeAsync();
                        await stream.DisposeAsync();
                    }
                }
            }

            if (labels != null)
            {
                form.Add(new StringContent(JsonConvert.SerializeObject(labels)), "labels");
            }

            var response = await Api.Client.PostAsync($"{GetEndpoint()}/{voice.Id}/edit", form, cancellationToken);
            await response.CheckResponseAsync(cancellationToken);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Delete a voice by its <see cref="Voice.Id"/>.
        /// </summary>
        /// <param name="voiceId">The id of the <see cref="Voice"/> to delete.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if voice was successfully deleted.</returns>
        public async Task<bool> DeleteVoiceAsync(string voiceId, CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.DeleteAsync($"{GetEndpoint()}/{voiceId}", cancellationToken);
            await response.CheckResponseAsync(cancellationToken);
            return response.IsSuccessStatusCode;
        }

        #region Samples

        /// <summary>
        /// Get the audio corresponding to a sample attached to a voice.
        /// </summary>
        /// <param name="voiceId">The <see cref="Voice"/> id this <see cref="Sample"/> belongs to.</param>
        /// <param name="sampleId">The <see cref="Sample"/> id to download.</param>
        /// <param name="saveDirectory">Optional, directory to save the <see cref="Sample"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="AudioClip"/>.</returns>
        public async Task<AudioClip> GetVoiceSampleAsync(string voiceId, string sampleId, string saveDirectory = null, CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync($"{GetEndpoint()}/{voiceId}/samples/{sampleId}/audio", cancellationToken);
            await response.CheckResponseAsync(cancellationToken);

            Rest.ValidateCacheDirectory();

            var rootDirectory = (saveDirectory ?? Rest.DownloadCacheDirectory).CreateNewDirectory(nameof(ElevenLabs));
            var downloadDirectory = rootDirectory.CreateNewDirectory(voiceId);
            var filePath = Path.Combine(downloadDirectory, $"{sampleId}.mp3");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var responseStream = await response.Content.ReadAsStreamAsync();

            try
            {
                var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);

                try
                {
                    await responseStream.CopyToAsync(fileStream, cancellationToken);
                    await fileStream.FlushAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    fileStream.Close();
                    await fileStream.DisposeAsync();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                await responseStream.DisposeAsync();
            }

            var audioClip = await Rest.DownloadAudioClipAsync($"file://{filePath}", AudioType.MPEG, cancellationToken: cancellationToken);
            return audioClip;
        }

        /// <summary>
        /// Delete the audio corresponding to a sample attached to a voice.
        /// </summary>
        /// <param name="voiceId">The <see cref="Voice"/> id this <see cref="Sample"/> belongs to.</param>
        /// <param name="sampleId">The <see cref="Sample"/> id to delete.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if <see cref="Voice"/> <see cref="Sample"/> was successfully deleted.</returns>
        public async Task<bool> DeleteVoiceSampleAsync(string voiceId, string sampleId, CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.DeleteAsync($"{GetEndpoint()}/{voiceId}/samples/{sampleId}", cancellationToken);
            await response.CheckResponseAsync(cancellationToken);
            return response.IsSuccessStatusCode;
        }

        #endregion Samples
    }
}
