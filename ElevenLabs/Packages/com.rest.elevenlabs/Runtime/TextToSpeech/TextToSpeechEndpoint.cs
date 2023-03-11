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
        public TextToSpeechEndpoint(ElevenLabsClient api) : base(api) { }

        protected override string GetEndpoint()
            => $"{Api.BaseUrl}text-to-speech";

        /// <summary>
        /// Converts text into speech using a voice of your choice and returns audio.
        /// </summary>
        /// <param name="text">Text input to synthesize speech for. Maximum 5000 characters.</param>
        /// <param name="voice"><see cref="Voice"/> to use.</param>
        /// <param name="voiceSettings">Optional, <see cref="VoiceSettings"/> that will override the default settings in <see cref="Voice.Settings"/>.</param>
        /// <param name="saveDirectory">Optional, save directory to save the audio clip. Defaults to <see cref="Rest.DownloadCacheDirectory"/></param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>Downloaded clip path, and the loaded audio clip.</returns>
        public async Task<Tuple<string, AudioClip>> TextToSpeechAsync(string text, Voice voice, VoiceSettings voiceSettings = null, string saveDirectory = null, CancellationToken cancellationToken = default)
        {
            if (text.Length > 5000)
            {
                throw new ArgumentOutOfRangeException(nameof(text), $"{nameof(text)} cannot exceed 5000 characters");
            }

            Rest.ValidateCacheDirectory();

            var rootDirectory = (saveDirectory ?? Rest.DownloadCacheDirectory).CreateNewDirectory(nameof(ElevenLabs));
            var downloadDirectory = rootDirectory.CreateNewDirectory("TextToSpeech");
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
            return new Tuple<string, AudioClip>(filePath, audioClip);
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
