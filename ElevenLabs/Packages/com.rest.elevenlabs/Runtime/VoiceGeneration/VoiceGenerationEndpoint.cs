// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using ElevenLabs.Voices;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.WebRequestRest;

namespace ElevenLabs.VoiceGeneration
{
    public sealed class VoiceGenerationEndpoint : BaseEndPoint
    {
        public VoiceGenerationEndpoint(ElevenLabsClient api) : base(api) { }

        protected override string Root => "voice-generation";

        /// <summary>
        /// Gets the available voice generation options.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="GeneratedVoiceOptions"/>.</returns>
        public async Task<GeneratedVoiceOptions> GetVoiceGenerationOptionsAsync(CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync(GetUrl("/generate-voice/parameters"), cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GeneratedVoiceOptions>(responseAsString, Api.JsonSerializationOptions);
        }

        /// <summary>
        /// Generate a <see cref="Voice"/>.
        /// </summary>
        /// <param name="generatedVoiceRequest"><see cref="GeneratedVoiceRequest"/></param>
        /// <param name="saveDirectory">The save directory for downloaded audio file.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Tuple{VoiceId,AudioClip}"/>.</returns>
        public async Task<Tuple<string, AudioClip>> GenerateVoiceAsync(GeneratedVoiceRequest generatedVoiceRequest, string saveDirectory = null, CancellationToken cancellationToken = default)
        {
            var payload = JsonConvert.SerializeObject(generatedVoiceRequest, Api.JsonSerializationOptions).ToJsonStringContent();
            var response = await Api.Client.PostAsync(GetUrl("/generate-voice"), payload, cancellationToken);
            await response.CheckResponseAsync();

            var generatedVoiceId = response.Headers.FirstOrDefault(pair => pair.Key == "generated_voice_id").Value.FirstOrDefault();

            await Rest.ValidateCacheDirectoryAsync();

            var rootDirectory = (saveDirectory ?? Rest.DownloadCacheDirectory).CreateNewDirectory(nameof(ElevenLabs));
            var downloadDirectory = rootDirectory.CreateNewDirectory(nameof(VoiceGeneration));
            var filePath = Path.Combine(downloadDirectory, $"{generatedVoiceId}.mp3");

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
            return new Tuple<string, AudioClip>(generatedVoiceId, audioClip);
        }

        /// <summary>
        /// Clone a <see cref="Voice"/>.
        /// </summary>
        /// <param name="createVoiceRequest"><see cref="CreateVoiceRequest"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Voice"/>.</returns>
        public async Task<Voice> CreateVoiceAsync(CreateVoiceRequest createVoiceRequest, CancellationToken cancellationToken = default)
        {
            var payload = JsonConvert.SerializeObject(createVoiceRequest, Api.JsonSerializationOptions).ToJsonStringContent();
            var response = await Api.Client.PostAsync(GetUrl("/create-voice"), payload, cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Voice>(responseAsString, Api.JsonSerializationOptions);
        }
    }
}
