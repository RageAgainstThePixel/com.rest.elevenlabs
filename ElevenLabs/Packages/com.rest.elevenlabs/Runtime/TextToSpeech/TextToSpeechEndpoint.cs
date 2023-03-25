// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Voices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.Async;
using Utilities.WebRequestRest;

namespace ElevenLabs.TextToSpeech
{
    /// <summary>
    /// Access to convert text to synthesized speech.
    /// </summary>
    public sealed class TextToSpeechEndpoint : BaseEndPoint
    {
        public TextToSpeechEndpoint(ElevenLabsClient api) : base(api) { }

        protected override string Root => "text-to-speech";

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

            if (voice == null)
            {
                throw new ArgumentNullException(nameof(voice));
            }

            await Rest.ValidateCacheDirectoryAsync();

            var rootDirectory = (saveDirectory ?? Rest.DownloadCacheDirectory).CreateNewDirectory(nameof(ElevenLabs));
            var speechToTextDirectory = rootDirectory.CreateNewDirectory(nameof(TextToSpeech));
            var downloadDirectory = speechToTextDirectory.CreateNewDirectory(voice.Name);
            var fileName = $"{text.GenerateGuid()}.mp3";
            var filePath = Path.Combine(downloadDirectory, fileName);

            if (!File.Exists(filePath))
            {
                var defaultVoiceSettings = voiceSettings ?? voice.Settings ?? await Api.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken);
                var payload = JsonConvert.SerializeObject(new TextToSpeechRequest(text, defaultVoiceSettings)).ToJsonStringContent();
                var response = await Api.Client.PostAsync(GetUrl($"/{voice.Id}"), payload, cancellationToken);
                await response.CheckResponseAsync();
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
        /// <param name="resultHandler">An action to be called when a new <see cref="AudioClip"/> part has arrived.</param>
        /// <param name="voiceSettings">Optional, <see cref="VoiceSettings"/> that will override the default settings in <see cref="Voice.Settings"/>.</param>
        /// <param name="saveDirectory">Optional, save directory to save the audio clip. Defaults to <see cref="Rest.DownloadCacheDirectory"/></param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        public async Task StreamTextToSpeechAsync(string text, Voice voice, Action<AudioClip> resultHandler, VoiceSettings voiceSettings = null, string saveDirectory = null, CancellationToken cancellationToken = default)
        {
            if (text.Length > 5000)
            {
                throw new ArgumentOutOfRangeException(nameof(text), $"{nameof(text)} cannot exceed 5000 characters");
            }

            if (voice == null)
            {
                throw new ArgumentNullException(nameof(voice));
            }

            await Rest.ValidateCacheDirectoryAsync();
            var rootDirectory = (saveDirectory ?? Rest.DownloadCacheDirectory).CreateNewDirectory(nameof(ElevenLabs));
            var speechToTextDirectory = rootDirectory.CreateNewDirectory(nameof(TextToSpeech));
            var voiceDirectory = speechToTextDirectory.CreateNewDirectory(voice.Name);
            var clipGuid = text.GenerateGuid().ToString();
            var downloadDirectory = voiceDirectory.CreateNewDirectory(clipGuid);
            var fileName = $"{clipGuid}.mp3";
            var filePath = Path.Combine(downloadDirectory, fileName);

            if (File.Exists(fileName))
            {
                var clip = await Rest.DownloadAudioClipAsync($"file://{filePath}", AudioType.MPEG, cancellationToken: cancellationToken);
                // Always raise event callbacks on main thread
                await Awaiters.UnityMainThread;
                resultHandler.Invoke(clip);
                return;
            }

            var defaultVoiceSettings = voiceSettings ?? voice.Settings ?? await Api.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken);
            var payload = JsonConvert.SerializeObject(new TextToSpeechRequest(text, defaultVoiceSettings), Api.JsonSerializationOptions).ToJsonStringContent();
            using var request = new HttpRequestMessage(HttpMethod.Post, GetUrl($"/{voice.Id}/streaming"))
            {
                Content = payload
            };
            using var response = await Api.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            await response.CheckResponseAsync();
            await using var stream = await response.Content.ReadAsStreamAsync();
            stream.ReadTimeout = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
            var buffer = new byte[1024];
            var audioClip = AudioClip.Create(clipGuid, 0, 1, 44100, false);
            var samples = new float[audioClip.samples];
            var position = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                    if (bytesRead == 0)
                    {
                        // No more data available for reading, but there may still be more coming
                        await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
                        continue;
                    }

                    // Convert byte array to float samples
                    var floatSamples = new float[bytesRead / 2];
                    for (var i = 0; i < floatSamples.Length; i++)
                    {
                        var sample = BitConverter.ToInt16(buffer, i * 2);

                        floatSamples[i] = sample / (float)short.MaxValue;
                    }

                    // Copy float samples to audio clip samples
                    Array.Copy(floatSamples, 0, samples, position, floatSamples.Length);
                    position += floatSamples.Length;

                    // If samples buffer is full, create new audio clip and send it to the result handler
                    if (position == samples.Length)
                    {
                        audioClip.SetData(samples, 0);
                        resultHandler(audioClip);
                        samples = new float[audioClip.samples];
                        position = 0;
                    }
                }
                catch (IOException ex) when (ex.InnerException is SocketException { SocketErrorCode: SocketError.TimedOut })
                {
                    // Stream has timed out, assume server has finished sending data
                    break;
                }
            }

            if (position > 0)
            {
                var actualSamples = new float[position];
                Array.Copy(samples, actualSamples, position);
                audioClip.SetData(actualSamples, 0);
                resultHandler(audioClip);
            }
        }

        /// <summary>
        /// Converts text into speech using a voice of your choice and returns audio as an audio stream.<br/>
        /// If you are not using C# 8 supporting IAsyncEnumerable{T} or if you are using the .NET Framework,
        /// you may need to use <see cref="StreamTextToSpeechAsync(string, Voice, Action{AudioClip}, VoiceSettings, string, CancellationToken)"/> instead.
        /// </summary>
        /// <param name="text">Text input to synthesize speech for.</param>
        /// <param name="voice"><see cref="Voice"/> to use.</param>
        /// <param name="voiceSettings">Optional, <see cref="VoiceSettings"/> that will override the default settings in <see cref="Voice.Settings"/>.</param>
        /// <param name="saveDirectory">Optional, save directory to save the audio clip. Defaults to <see cref="Rest.DownloadCacheDirectory"/></param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="AudioClip"/> part.</returns>
        public async IAsyncEnumerable<AudioClip> StreamTextToSpeechEnumerableAsync(string text, Voice voice, VoiceSettings voiceSettings = null, string saveDirectory = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (text.Length > 5000)
            {
                throw new ArgumentOutOfRangeException(nameof(text), $"{nameof(text)} cannot exceed 5000 characters");
            }

            if (voice == null)
            {
                throw new ArgumentNullException(nameof(voice));
            }

            await Rest.ValidateCacheDirectoryAsync();
            var rootDirectory = (saveDirectory ?? Rest.DownloadCacheDirectory).CreateNewDirectory(nameof(ElevenLabs));
            var downloadDirectory = rootDirectory.CreateNewDirectory(nameof(TextToSpeech));

            var defaultVoiceSettings = voiceSettings ?? voice.Settings ?? await Api.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken);
            var payload = JsonConvert.SerializeObject(new TextToSpeechRequest(text, defaultVoiceSettings)).ToJsonStringContent();

            await Task.CompletedTask;
            yield return null;
        }
    }
}
