// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Voices;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.WebRequestRest;

namespace ElevenLabs
{
    public sealed class VoiceGenerationEndpoint : BaseEndPoint
    {
        public VoiceGenerationEndpoint(ElevenLabsClient api) : base(api) { }

        protected override string GetEndpoint()
            => $"{Api.BaseUrl}voice-generation";

        public async Task<GeneratedVoiceOptions> GetVoiceGenerationOptionsAsync(CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync($"{GetEndpoint()}/generate-voice/parameters", cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GeneratedVoiceOptions>(responseAsString, Api.JsonSerializationOptions);
        }

        public async Task<bool> GenerateVoiceAsync(GeneratedVoiceRequest generatedVoiceRequest, CancellationToken cancellationToken = default)
        {
            var payload = JsonConvert.SerializeObject(generatedVoiceRequest, Api.JsonSerializationOptions).ToJsonStringContent();
            var response = await Api.Client.PostAsync($"{GetEndpoint()}/generate-voice", payload, cancellationToken);
            await response.CheckResponseAsync(cancellationToken);

            var generatedVoiceId = response.Headers.FirstOrDefault(pair => pair.Key == "generated_voice_id").Value.FirstOrDefault();

            Rest.ValidateCacheDirectory();

            var rootDirectory = Path.Combine(Rest.DownloadCacheDirectory, nameof(ElevenLabs));

            if (!Directory.Exists(rootDirectory))
            {
                Directory.CreateDirectory(rootDirectory);
            }

            var downloadDirectory = Path.Combine(rootDirectory, "VoiceGeneration");
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
            return audioClip;
        }

        public async Task<Voice> CreateVoiceAsync(CreateVoiceRequest createVoiceRequest, CancellationToken cancellationToken = default)
        {
            var payload = JsonConvert.SerializeObject(createVoiceRequest).ToJsonStringContent();
            var response = await Api.Client.PostAsync($"{GetEndpoint()}/create-voice", payload, cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Voice>(responseAsString, Api.JsonSerializationOptions);
        }
    }
}
