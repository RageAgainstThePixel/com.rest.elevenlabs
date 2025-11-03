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
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<GeneratedVoiceOptions>(response.Body, ElevenLabsClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Generate a <see cref="Voice"/>.
        /// </summary>
        /// <param name="generatedVoicePreviewRequest"><see cref="GeneratedVoicePreviewRequest"/></param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A voice id, and preview clip. <see cref="Tuple{VoiceId,AudioClip}"/>.</returns>
        public async Task<Tuple<string, AudioClip>> GenerateVoicePreviewAsync(GeneratedVoicePreviewRequest generatedVoicePreviewRequest, CancellationToken cancellationToken = default)
        {
            var payload = JsonConvert.SerializeObject(generatedVoicePreviewRequest, ElevenLabsClient.JsonSerializationOptions);
            var response = await Rest.PostAsync(GetUrl("/generate-voice"), payload, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);

            await Rest.ValidateCacheDirectoryAsync();
            var generatedVoiceId = response.Headers["generated_voice_id"];
            var cacheDirectory = Rest.DownloadCacheDirectory
                .CreateNewDirectory(nameof(ElevenLabs))
                .CreateNewDirectory(nameof(VoiceGeneration));
            var cachedPath = Path.Combine(cacheDirectory, $"{generatedVoiceId}.wav");

            if (File.Exists(cachedPath))
            {
                File.Delete(cachedPath);
            }

            await File.WriteAllBytesAsync(cachedPath, response.Data, cancellationToken).ConfigureAwait(true);
            var audioClip = await Rest.DownloadAudioClipAsync($"file://{cachedPath}", AudioType.WAV, parameters: new RestParameters(debug: EnableDebug), cancellationToken: cancellationToken);
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
            var payload = JsonConvert.SerializeObject(createVoiceRequest, ElevenLabsClient.JsonSerializationOptions);
            var response = await Rest.PostAsync(GetUrl("/create-voice"), payload, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<Voice>(response.Body, ElevenLabsClient.JsonSerializationOptions);
        }
    }
}
