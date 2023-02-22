// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Voices;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.WebRequestRest;

namespace ElevenLabs.TextToSpeech
{
    /// <summary>
    /// Access to convert text to synthesized speech.
    /// </summary>
    public sealed class TextToSpeechEndpoint : BaseEndPoint
    {
        private class TextToSpeechRequest
        {
            public TextToSpeechRequest(
                [JsonProperty("text")] string text,
                [JsonProperty("voice_settings")] VoiceSettings voiceSettings)
            {
                Text = text;
                VoiceSettings = voiceSettings;
            }

            [JsonProperty("text")]
            public string Text { get; }

            [JsonProperty("voice_settings")]
            public VoiceSettings VoiceSettings { get; }
        }

        public TextToSpeechEndpoint(ElevenLabsClient api) : base(api) { }

        protected override string GetEndpoint()
            => $"{Api.BaseUrl}text-to-speech";

        /// <summary>
        /// Converts text into speech using a voice of your choice and returns audio.
        /// </summary>
        /// <param name="text">Text input to synthesize speech for.</param>
        /// <param name="voice"><see cref="Voice"/> to use.</param>
        /// <param name="voiceSettings">Optional, <see cref="VoiceSettings"/> that will override the default settings in <see cref="Voice.Settings"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="AudioClip"/>.</returns>
        public async Task<AudioClip> TextToSpeechAsync(string text, Voice voice, VoiceSettings voiceSettings = null, CancellationToken cancellationToken = default)
        {
            Rest.ValidateCacheDirectory();

            var rootDirectory = Path.Combine(Rest.DownloadCacheDirectory, nameof(ElevenLabs));

            if (!Directory.Exists(rootDirectory))
            {
                Directory.CreateDirectory(rootDirectory);
            }

            var downloadDirectory = Path.Combine(rootDirectory, "TextToSpeech");
            var fileName = $"{text.GenerateGuid()}.mp3";
            var filePath = Path.Combine(downloadDirectory, fileName);

            if (!File.Exists(filePath))
            {
                var defaultVoiceSettings = voiceSettings ?? voice.Settings ?? await Api.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken);
                var payload = JsonConvert.SerializeObject(new TextToSpeechRequest(text, defaultVoiceSettings)).ToJsonStringContent();
                var response = await Api.Client.PostAsync($"{GetEndpoint()}/{voice.Id}", payload, cancellationToken);
                await response.CheckResponseAsync(cancellationToken);
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
            }

            var audioClip = await Rest.DownloadAudioClipAsync($"file://{filePath}", AudioType.MPEG, cancellationToken: cancellationToken);
            return audioClip;
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
