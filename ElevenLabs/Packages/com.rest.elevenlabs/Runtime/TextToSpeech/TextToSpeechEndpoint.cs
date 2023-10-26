// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using ElevenLabs.Models;
using ElevenLabs.Voices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Utilities.Async;
using Utilities.Audio;
using Utilities.Encoding.OggVorbis;
using Utilities.WebRequestRest;
using Debug = UnityEngine.Debug;

namespace ElevenLabs.TextToSpeech
{
    /// <summary>
    /// Access to convert text to synthesized speech.
    /// </summary>
    public sealed class TextToSpeechEndpoint : ElevenLabsBaseEndPoint
    {
        private const string PCMFormat = "pcm_44100";
        private const string OutputFormat = "output_format";
        private const string StreamingLatency = "optimize_streaming_latency";
        private const string HistoryItemId = "history-item-id";

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
        /// <param name="cancellationToken">
        /// Optional, <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>Downloaded clip path, and the loaded audio clip.</returns>
        public async Task<DownloadItem> TextToSpeechAsync(string text, Voice voice, VoiceSettings voiceSettings = null, Model model = null, int? optimizeStreamingLatency = null, CancellationToken cancellationToken = default)
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

            var downloadDirectory = await GetCacheDirectoryAsync(voice);
            var defaultVoiceSettings = voiceSettings ?? voice.Settings ?? await client.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken);
            var request = new TextToSpeechRequest(text, model, defaultVoiceSettings);
            var payload = JsonConvert.SerializeObject(request, ElevenLabsClient.JsonSerializationOptions);
            var parameters = new Dictionary<string, string>();

            if (optimizeStreamingLatency.HasValue)
            {
                parameters.Add(StreamingLatency, optimizeStreamingLatency.Value.ToString());
            }

            var response = await Rest.PostAsync(GetUrl($"/{voice.Id}", parameters), payload, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);

            if (!response.Headers.TryGetValue(HistoryItemId, out var clipId))
            {
                throw new ArgumentException("Failed to find history item id!");
            }

            var responseStream = new MemoryStream(response.Data);
            var cachedPath = $"{downloadDirectory}/{clipId}.mp3";
            var fileStream = new FileStream(cachedPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);

            try
            {
                await responseStream.CopyToAsync(fileStream, cancellationToken);
                await fileStream.FlushAsync(cancellationToken);
            }
            finally
            {
                fileStream.Close();
                await fileStream.DisposeAsync();
                await responseStream.DisposeAsync();
            }

            var audioClip = await Rest.DownloadAudioClipAsync($"file://{cachedPath}", AudioType.MPEG, cancellationToken: cancellationToken);
            return new DownloadItem(clipId, text, voice, audioClip, cachedPath);
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
        /// <param name="partialClipCallback">
        /// An callback that contains a partial response with an <see cref="AudioClip"/> that is ready to play.
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
        /// <param name="cancellationToken">
        /// Optional, <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>Downloaded clip path, and the loaded audio clip.</returns>
        public async Task<DownloadItem> StreamTextToSpeechAsync(string text, Voice voice, Action<AudioClip> partialClipCallback, VoiceSettings voiceSettings = null, Model model = null, int? optimizeStreamingLatency = null, CancellationToken cancellationToken = default)
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

            var downloadDirectory = await GetCacheDirectoryAsync(voice);
            var defaultVoiceSettings = voiceSettings ?? voice.Settings ?? await client.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken);
            var request = new TextToSpeechRequest(text, model, defaultVoiceSettings);
            var payload = JsonConvert.SerializeObject(request, ElevenLabsClient.JsonSerializationOptions);
            var parameters = new Dictionary<string, string>
            {
                { OutputFormat, PCMFormat }
            };

            if (optimizeStreamingLatency.HasValue)
            {
                parameters.Add(StreamingLatency, optimizeStreamingLatency.Value.ToString());
            }

            var responseStream = new MemoryStream();
            var part = 0;

            try
            {
                var response = await Rest.PostAsync(GetUrl($"/{voice.Id}/stream", parameters), payload, StreamCallback, eventChunkSize: 512, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
                response.Validate(EnableDebug);

                if (!response.Headers.TryGetValue(HistoryItemId, out var clipId))
                {
                    throw new ArgumentException("Failed to find history item id!");
                }

                var pcmData = PCMEncoder.Decode(responseStream.ToArray(), PCMFormatSize.SixteenBit);
                var fullClip = AudioClip.Create(clipId, pcmData.Length, 1, 44100, false);

                if (!fullClip.SetData(pcmData, 0))
                {
                    throw new Exception("Failed to set pcm data!");
                }

                var oggBytes = await OggEncoder.ConvertToBytesAsync(pcmData, 44100, 1, cancellationToken: cancellationToken).ConfigureAwait(false);
                var cachedPath = $"{downloadDirectory}/{clipId}.ogg";
                await File.WriteAllBytesAsync(cachedPath, oggBytes, cancellationToken: cancellationToken).ConfigureAwait(false);
                await Awaiters.UnityMainThread;
                return new DownloadItem(clipId, text, voice, fullClip, cachedPath);
            }
            finally
            {
                await responseStream.DisposeAsync().ConfigureAwait(true);
            }

            async void StreamCallback(UnityWebRequest webRequest, byte[] bytes)
            {
                await Awaiters.UnityMainThread;

                if (!webRequest.GetResponseHeaders().TryGetValue(HistoryItemId, out var clipId))
                {
                    throw new ArgumentException("Failed to find history item id!");
                }

                var pcmData = PCMEncoder.Decode(bytes, PCMFormatSize.SixteenBit);
                var audioClip = AudioClip.Create($"{clipId}_{++part}", pcmData.Length, 1, 44100, false);

                if (!audioClip.SetData(pcmData, 0))
                {
                    Debug.LogError("Failed to set pcm data to partial clip.");
                    return;
                }

                await responseStream.WriteAsync(bytes, cancellationToken).ConfigureAwait(true);
                partialClipCallback.Invoke(audioClip);
            }
        }

        private static async Task<string> GetCacheDirectoryAsync(Voice voice)
        {
            await Rest.ValidateCacheDirectoryAsync();
            return Rest.DownloadCacheDirectory
                .CreateNewDirectory(nameof(ElevenLabs))
                .CreateNewDirectory(nameof(TextToSpeech))
                .CreateNewDirectory(voice.Id);
        }
    }
}
