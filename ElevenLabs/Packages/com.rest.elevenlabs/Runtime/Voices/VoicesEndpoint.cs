// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Utilities.Async;
using Utilities.Encoding.Wav;
using Utilities.WebRequestRest;

namespace ElevenLabs.Voices
{
    /// <summary>
    /// Access to voices created either by you or us.
    /// </summary>
    public sealed class VoicesEndpoint : ElevenLabsBaseEndPoint
    {
        [Preserve]
        private class VoiceResponse
        {
            [Preserve]
            [JsonConstructor]
            public VoiceResponse([JsonProperty("voice_id")] string voiceId)
            {
                VoiceId = voiceId;
            }

            [Preserve]
            [JsonProperty("voice_id")]
            public string VoiceId { get; }
        }

        public VoicesEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "voices";

        /// <summary>
        /// Gets a list of all available voices for a user, and downloads all their settings.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="IReadOnlyList{T}"/> of <see cref="Voice"/>s.</returns>
        public Task<IReadOnlyList<Voice>> GetAllVoicesAsync(CancellationToken cancellationToken = default)
            => GetAllVoicesAsync(true, cancellationToken);

        /// <summary>
        /// Gets a list of all available voices for a user.
        /// </summary>
        /// <param name="downloadSettings">Whether to download all settings for the voices.</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="IReadOnlyList{T}"/> of <see cref="Voice"/>s.</returns>
        public async Task<IReadOnlyList<Voice>> GetAllVoicesAsync(bool downloadSettings, CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl(), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            var voices = JsonConvert.DeserializeObject<VoiceList>(response.Body, ElevenLabsClient.JsonSerializationOptions).Voices;

            if (downloadSettings)
            {
                var voiceSettingsTasks = new List<Task>();

                foreach (var voice in voices)
                {
                    voiceSettingsTasks.Add(LocalGetVoiceSettingsAsync());

                    async Task LocalGetVoiceSettingsAsync()
                    {
                        await Awaiters.UnityMainThread;
                        voice.Settings = await GetVoiceSettingsAsync(voice, cancellationToken).ConfigureAwait(true);
                    }
                }

                await Task.WhenAll(voiceSettingsTasks).ConfigureAwait(true);
            }

            return voices.ToList();
        }

        /// <summary>
        /// Gets the default settings for voices.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceSettings"/>.</returns>
        public async Task<VoiceSettings> GetDefaultVoiceSettingsAsync(CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl("/settings/default"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<VoiceSettings>(response.Body, ElevenLabsClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Gets the settings for a specific voice.
        /// </summary>
        /// <param name="voiceId">The id of the <see cref="Voice"/> to get <see cref="VoiceSettings"/> for.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceSettings"/>.</returns>
        public async Task<VoiceSettings> GetVoiceSettingsAsync(string voiceId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(voiceId))
            {
                throw new ArgumentNullException(nameof(voiceId));
            }

            var response = await Rest.GetAsync(GetUrl($"/{voiceId}/settings"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<VoiceSettings>(response.Body, ElevenLabsClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Gets metadata about a specific voice.
        /// </summary>
        /// <param name="voiceId">The id of the <see cref="Voice"/> to get.</param>
        /// <param name="withSettings">Should the response include the <see cref="VoiceSettings"/>?</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Voice"/>.</returns>
        public async Task<Voice> GetVoiceAsync(string voiceId, bool withSettings = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(voiceId))
            {
                throw new ArgumentNullException(nameof(voiceId));
            }

            var response = await Rest.GetAsync(GetUrl($"/{voiceId}?with_settings={withSettings.ToString().ToLower()}"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<Voice>(response.Body, ElevenLabsClient.JsonSerializationOptions);
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
            if (string.IsNullOrWhiteSpace(voiceId))
            {
                throw new ArgumentNullException(nameof(voiceId));
            }

            var payload = JsonConvert.SerializeObject(voiceSettings, ElevenLabsClient.JsonSerializationOptions);
            var response = await Rest.PostAsync(GetUrl($"/{voiceId}/settings/edit"), payload, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return response.Successful;
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
            var form = new WWWForm();

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            form.AddField("name", name);

            if (samplePaths != null)
            {
                var paths = samplePaths.Where(path => !string.IsNullOrWhiteSpace(path)).ToList();

                if (paths.Any())
                {
                    foreach (var sample in paths)
                    {
                        if (!File.Exists(sample))
                        {
                            Debug.LogError($"No sample clip found at {sample}!");
                            continue;
                        }

                        try
                        {
                            var fileBytes = await File.ReadAllBytesAsync(sample, cancellationToken);

                            if (fileBytes.Length > 0)
                            {
                                form.AddBinaryData("files", fileBytes, Path.GetFileName(sample));
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                }
            }

            if (labels != null)
            {
                form.AddField("labels", JsonConvert.SerializeObject(labels, ElevenLabsClient.JsonSerializationOptions));
            }

            var response = await Rest.PostAsync(GetUrl("/add"), form, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            var voiceResponse = JsonConvert.DeserializeObject<VoiceResponse>(response.Body, ElevenLabsClient.JsonSerializationOptions);
            return await GetVoiceAsync(voiceResponse.VoiceId, cancellationToken: cancellationToken);
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
            var form = new WWWForm();

            if (voice == null)
            {
                throw new ArgumentNullException(nameof(voice));
            }

            form.AddField("name", voice.Name);

            if (samplePaths != null)
            {
                var paths = samplePaths.Where(path => !string.IsNullOrWhiteSpace(path)).ToList();

                if (paths.Any())
                {
                    foreach (var sample in paths)
                    {
                        if (!File.Exists(sample))
                        {
                            Debug.LogError($"No sample clip found at {sample}!");
                            continue;
                        }

                        try
                        {
                            var fileBytes = await File.ReadAllBytesAsync(sample, cancellationToken);
                            form.AddBinaryData("files", fileBytes, Path.GetFileName(sample));
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                }
            }

            if (labels != null)
            {
                form.AddField("labels", JsonConvert.SerializeObject(labels, ElevenLabsClient.JsonSerializationOptions));
            }

            var response = await Rest.PostAsync(GetUrl($"/{voice.Id}/edit"), form, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return response.Successful;
        }

        /// <summary>
        /// Delete a voice by its <see cref="Voice.Id"/>.
        /// </summary>
        /// <param name="voiceId">The id of the <see cref="Voice"/> to delete.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if voice was successfully deleted.</returns>
        public async Task<bool> DeleteVoiceAsync(string voiceId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(voiceId))
            {
                throw new ArgumentNullException(nameof(voiceId));
            }

            var response = await Rest.DeleteAsync(GetUrl($"/{voiceId}"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return response.Successful;
        }

        #region Samples

        /// <summary>
        /// Download the audio corresponding to a <see cref="Sample"/> attached to a <see cref="Voice"/>.
        /// </summary>
        /// <param name="voice">The <see cref="Voice"/> this <see cref="Sample"/> belongs to.</param>
        /// <param name="sample">The <see cref="Sample"/> id to download.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceClip"/>.</returns>
        public async Task<VoiceClip> DownloadVoiceSampleAudioAsync(Voice voice, Sample sample, CancellationToken cancellationToken = default)
        {
            if (voice == null ||
                string.IsNullOrWhiteSpace(voice.Id))
            {
                throw new ArgumentNullException(nameof(voice));
            }

            if (sample == null ||
                string.IsNullOrWhiteSpace(sample.Id))
            {
                throw new ArgumentNullException(nameof(sample));
            }

            await Rest.ValidateCacheDirectoryAsync();
            var downloadDirectory = Rest.DownloadCacheDirectory
                .CreateNewDirectory(nameof(ElevenLabs))
                .CreateNewDirectory(voice.Id)
                .CreateNewDirectory("Samples");
            // TODO possibly handle other types?
            var audioType = sample.MimeType.Contains("mpeg") ? AudioType.MPEG : AudioType.WAV;
            var extension = audioType switch
            {
                AudioType.MPEG => "mp3",
                AudioType.WAV => "wav",
                _ => throw new ArgumentOutOfRangeException($"Unsupported {nameof(AudioType)}: {audioType}")
            };
            var cachedPath = Path.Combine(downloadDirectory, $"{sample.Id}.{extension}");

            if (!File.Exists(cachedPath))
            {
                var response = await Rest.GetAsync(GetUrl($"/{voice.Id}/samples/{sample.Id}/audio"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
                response.Validate(EnableDebug);

                switch (audioType)
                {
                    case AudioType.MPEG:
                        await File.WriteAllBytesAsync(cachedPath, response.Data, cancellationToken).ConfigureAwait(false);
                        break;
                    case AudioType.WAV:
                        var sampleRate = 44100; // TODO unknown sample rate.
                        await WavEncoder.WriteToFileAsync(cachedPath, response.Data, 1, sampleRate, cancellationToken: cancellationToken).ConfigureAwait(false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Unsupported {nameof(AudioType)}: {audioType}");
                }
            }

            var audioClip = await Rest.DownloadAudioClipAsync($"file://{cachedPath}", audioType, parameters: new RestParameters(debug: EnableDebug), cancellationToken: cancellationToken);
            await Awaiters.UnityMainThread;
            return new VoiceClip(sample.Id, string.Empty, voice, audioClip, cachedPath);
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
            if (string.IsNullOrWhiteSpace(voiceId))
            {
                throw new ArgumentNullException(nameof(voiceId));
            }

            if (string.IsNullOrWhiteSpace(sampleId))
            {
                throw new ArgumentNullException(nameof(sampleId));
            }

            var response = await Rest.DeleteAsync(GetUrl($"/{voiceId}/samples/{sampleId}"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return response.Successful;
        }

        #endregion Samples
    }
}
