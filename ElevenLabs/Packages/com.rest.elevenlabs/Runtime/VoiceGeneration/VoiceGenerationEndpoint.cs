// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using ElevenLabs.Voices;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.WebRequestRest;

namespace ElevenLabs.VoiceGeneration
{
    public sealed class VoiceGenerationEndpoint : ElevenLabsBaseEndPoint
    {
        public VoiceGenerationEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "voice-generation";

        /// <summary>
        /// Gets the available voice generation options.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="GeneratedVoiceOptions"/>.</returns>
        public async Task<GeneratedVoiceOptions> GetVoiceGenerationOptionsAsync(CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl("/generate-voice/parameters"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.ValidateResponse();
            return JsonConvert.DeserializeObject<GeneratedVoiceOptions>(response.Body, client.JsonSerializationOptions);
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
            var payload = JsonConvert.SerializeObject(generatedVoiceRequest, client.JsonSerializationOptions);
            var response = await Rest.PostAsync(GetUrl("/generate-voice"), payload, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.ValidateResponse();
            var generatedVoiceId = response.Headers["generated_voice_id"];

            await Rest.ValidateCacheDirectoryAsync();

            var rootDirectory = (saveDirectory ?? Rest.DownloadCacheDirectory).CreateNewDirectory(nameof(ElevenLabs));
            var downloadDirectory = rootDirectory.CreateNewDirectory(nameof(VoiceGeneration));
            var filePath = Path.Combine(downloadDirectory, $"{generatedVoiceId}.mp3");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var responseStream = new MemoryStream(response.Data);

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

            var audioClip = await Rest.DownloadAudioClipAsync($"file://{filePath}", AudioType.MPEG, parameters: null, cancellationToken: cancellationToken);
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
            var payload = JsonConvert.SerializeObject(createVoiceRequest, client.JsonSerializationOptions);
            var response = await Rest.PostAsync(GetUrl("/create-voice"), payload, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.ValidateResponse();
            return JsonConvert.DeserializeObject<Voice>(response.Body, client.JsonSerializationOptions);
        }
    }
}
