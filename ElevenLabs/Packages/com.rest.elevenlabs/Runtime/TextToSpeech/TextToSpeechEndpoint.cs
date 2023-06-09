// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using ElevenLabs.Models;
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
    public sealed class TextToSpeechEndpoint : ElevenLabsBaseEndPoint
    {
        public TextToSpeechEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "text-to-speech";

        /// <summary>
        /// Converts text into speech using a voice of your choice and returns audio.
        /// </summary>
        /// <param name="text">
        /// Text input to synthesize speech for. Maximum 5000 characters.
        /// </param>
        /// <param name="voice">
        /// <see cref="Voice"/> to use.
        /// </param>
        /// <param name="voiceSettings">
        /// Optional, <see cref="VoiceSettings"/> that will override the default settings in <see cref="Voice.Settings"/>.
        /// </param>
        /// <param name="model">
        /// Optional, <see cref="Model"/> to use. Defaults to <see cref="Model.MonoLingualV1"/>.
        /// </param>
        /// <param name="optimizeStreamingLatency">
        /// Optional, You can turn on latency optimizations at some cost of quality.
        /// The best possible final latency varies by model.<br/>
        /// Possible values:<br/>
        /// 0 - default mode (no latency optimizations)<br/>
        /// 1 - normal latency optimizations (about 50% of possible latency improvement of option 3)<br/>
        /// 2 - strong latency optimizations (about 75% of possible latency improvement of option 3)<br/>
        /// 3 - max latency optimizations<br/>
        /// 4 - max latency optimizations, but also with text normalizer turned off for even more latency savings
        /// (best latency, but can mispronounce eg numbers and dates).
        /// </param>
        /// <param name="saveDirectory">
        /// Optional, The save directory to save the audio clip. Defaults to <see cref="Rest.DownloadCacheDirectory"/>.
        /// </param>
        /// <param name="deleteCachedFile">
        /// Optional, deletes the cached file for this text string. Default is false.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional, <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>Downloaded clip path, and the loaded audio clip.</returns>
        public async Task<Tuple<string, AudioClip>> TextToSpeechAsync(string text, Voice voice, VoiceSettings voiceSettings = null, Model model = null, int? optimizeStreamingLatency = null, string saveDirectory = null, bool deleteCachedFile = false, CancellationToken cancellationToken = default)
        {
            if (text.Length > 5000)
            {
                throw new ArgumentOutOfRangeException(nameof(text), $"{nameof(text)} cannot exceed 5000 characters");
            }

            if (voice == null ||
                string.IsNullOrWhiteSpace(voice.Id))
            {
                throw new ArgumentNullException(nameof(voice));
            }

            if (string.IsNullOrWhiteSpace(voice.Name))
            {
                Debug.LogWarning("Voice details not found! To speed up this call, cache the voice details before making this request.");
                voice = await client.VoicesEndpoint.GetVoiceAsync(voice, cancellationToken: cancellationToken);
            }

            await Rest.ValidateCacheDirectoryAsync();

            var rootDirectory = (saveDirectory ?? Rest.DownloadCacheDirectory).CreateNewDirectory(nameof(ElevenLabs));
            var speechToTextDirectory = rootDirectory.CreateNewDirectory(nameof(TextToSpeech));
            var downloadDirectory = speechToTextDirectory.CreateNewDirectory(voice.Name);
            var clipGuid = $"{voice.Id}{text}".GenerateGuid().ToString();
            var fileName = $"{clipGuid}.mp3";
            var filePath = Path.Combine(downloadDirectory, fileName);

            if (File.Exists(filePath))
            {
#if UNITY_EDITOR
                if (!deleteCachedFile && !UnityEditor.EditorApplication.isPlaying)
                {
                    deleteCachedFile = UnityEditor.EditorUtility.DisplayDialog(
                        "Attention!",
                        "You've already previously generated an audio clip with this same voice and text string.\n" +
                        "Do you want to create a new unique clip?\n\nThis will delete your old clip.", "Delete", "Cancel");
                }
#endif
                if (deleteCachedFile)
                {
                    File.Delete(filePath);
                }
            }

            if (!File.Exists(filePath))
            {
                var defaultVoiceSettings = voiceSettings ?? voice.Settings ?? await client.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken);
                var request = new TextToSpeechRequest(text, model, defaultVoiceSettings);
                var payload = JsonConvert.SerializeObject(request, client.JsonSerializationOptions);
                var endpoint = GetUrl($"/{voice.Id}{(optimizeStreamingLatency.HasValue ? $"?optimize_streaming_latency={optimizeStreamingLatency.Value}" : string.Empty)}");
                var response = await Rest.PostAsync(endpoint, payload, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
                response.Validate();
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
            }

            var audioClip = await Rest.DownloadAudioClipAsync($"file://{filePath}", AudioType.MPEG, parameters: null, cancellationToken: cancellationToken);
            return new Tuple<string, AudioClip>(filePath, audioClip);
        }

        /// <summary>
        /// Converts text into speech using a voice of your choice and returns audio as an audio stream.
        /// </summary>
        /// <param name="text">
        /// Text input to synthesize speech for. Maximum 5000 characters.
        /// </param>
        /// <param name="voice">
        /// <see cref="Voice"/> to use.
        /// </param>
        /// <param name="resultHandler">An action to be called when a new <see cref="AudioClip"/> the clip is ready to play.</param>
        /// <param name="voiceSettings">
        /// Optional, <see cref="VoiceSettings"/> that will override the default settings in <see cref="Voice.Settings"/>.
        /// </param>
        /// <param name="model">
        /// Optional, <see cref="Model"/> to use. Defaults to <see cref="Model.MonoLingualV1"/>.
        /// </param>
        /// <param name="optimizeStreamingLatency">
        /// Optional, You can turn on latency optimizations at some cost of quality.
        /// The best possible final latency varies by model.<br/>
        /// Possible values:<br/>
        /// 0 - default mode (no latency optimizations)<br/>
        /// 1 - normal latency optimizations (about 50% of possible latency improvement of option 3)<br/>
        /// 2 - strong latency optimizations (about 75% of possible latency improvement of option 3)<br/>
        /// 3 - max latency optimizations<br/>
        /// 4 - max latency optimizations, but also with text normalizer turned off for even more latency savings
        /// (best latency, but can mispronounce eg numbers and dates).
        /// </param>
        /// <param name="saveDirectory">
        /// Optional, The save directory to save the audio clip. Defaults to <see cref="Rest.DownloadCacheDirectory"/>.
        /// </param>
        /// <param name="deleteCachedFile">
        /// Optional, deletes the cached file for this text string. Default is false.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional, <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>Downloaded clip path, and the loaded audio clip.</returns>
        public async Task<Tuple<string, AudioClip>> StreamTextToSpeechAsync(string text, Voice voice, Action<AudioClip> resultHandler, VoiceSettings voiceSettings = null, Model model = null, int? optimizeStreamingLatency = null, string saveDirectory = null, bool deleteCachedFile = false, CancellationToken cancellationToken = default)
        {
            if (text.Length > 5000)
            {
                throw new ArgumentOutOfRangeException(nameof(text), $"{nameof(text)} cannot exceed 5000 characters");
            }

            if (voice == null)
            {
                throw new ArgumentNullException(nameof(voice));
            }

            if (string.IsNullOrWhiteSpace(voice.Name))
            {
                Debug.LogWarning("Voice details not found! To speed up this call, cache the voice details before making this request.");
                voice = await client.VoicesEndpoint.GetVoiceAsync(voice, cancellationToken: cancellationToken);
            }

            await Rest.ValidateCacheDirectoryAsync();

            var rootDirectory = (saveDirectory ?? Rest.DownloadCacheDirectory).CreateNewDirectory(nameof(ElevenLabs));
            var speechToTextDirectory = rootDirectory.CreateNewDirectory(nameof(TextToSpeech));
            var downloadDirectory = speechToTextDirectory.CreateNewDirectory(voice.Name);
            var clipGuid = $"{voice.Id}{text}".GenerateGuid().ToString();
            var fileName = $"{clipGuid}.mp3";
            var filePath = Path.Combine(downloadDirectory, fileName);
            AudioClip audioClip;

            if (File.Exists(filePath))
            {
#if UNITY_EDITOR
                if (!deleteCachedFile && !UnityEditor.EditorApplication.isPlaying)
                {
                    deleteCachedFile = UnityEditor.EditorUtility.DisplayDialog(
                        "Attention!",
                        "You've already previously generated an audio clip with this same voice and text string.\n" +
                        "Do you want to create a new unique clip?\n\nThis will delete your old clip.", "Delete", "Cancel");
                }
#endif
                if (deleteCachedFile)
                {
                    File.Delete(filePath);
                }
            }

            if (!File.Exists(filePath))
            {
                var defaultVoiceSettings = voiceSettings ?? voice.Settings ?? await client.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken);
                var request = new TextToSpeechRequest(text, model, defaultVoiceSettings);
                var payload = JsonConvert.SerializeObject(request, client.JsonSerializationOptions);
                var endpoint = GetUrl($"/{voice.Id}/stream{(optimizeStreamingLatency.HasValue ? $"?optimize_streaming_latency={optimizeStreamingLatency.Value}" : string.Empty)}");
                audioClip = await Rest.StreamAudioAsync(endpoint, AudioType.MPEG, resultHandler, jsonData: payload, parameters: new RestParameters(client.DefaultRequestHeaders, new Progress<Progress>(progress =>
                {
                    Debug.Log($"{progress.Speed} {progress.Unit} | {progress.Percentage}% | Length: {progress.Length} | Position: {progress.Position}");
                })), cancellationToken: cancellationToken);
            }
            else
            {
                audioClip = await Rest.StreamAudioAsync($"file://{filePath}", AudioType.MPEG, resultHandler, cancellationToken: cancellationToken);
            }

            return new Tuple<string, AudioClip>(filePath, audioClip);
        }
    }
}
