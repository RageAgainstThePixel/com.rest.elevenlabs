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

namespace ElevenLabs.TextToSpeech
{
    /// <summary>
    /// Access to convert text to synthesized speech.
    /// </summary>
    public sealed class TextToSpeechEndpoint : ElevenLabsBaseEndPoint
    {
        private const string HistoryItemId = "history-item-id";
        private const string OutputFormatParameter = "output_format";
        private const string OptimizeStreamingLatencyParameter = "optimize_streaming_latency";

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
        /// <param name="outputFormat">
        /// Output format of the generated audio.<br/>
        /// Defaults to <see cref="OutputFormat.MP3_44100_128"/>
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
        public async Task<VoiceClip> TextToSpeechAsync(string text, Voice voice, VoiceSettings voiceSettings = null, Model model = null, OutputFormat outputFormat = OutputFormat.MP3_44100_128, int? optimizeStreamingLatency = null, CancellationToken cancellationToken = default)
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
                { OutputFormatParameter, outputFormat.ToString().ToLower() }
            };

            if (optimizeStreamingLatency.HasValue)
            {
                parameters.Add(OptimizeStreamingLatencyParameter, optimizeStreamingLatency.Value.ToString());
            }

            var response = await Rest.PostAsync(GetUrl($"/{voice.Id}", parameters), payload, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);

            if (!response.Headers.TryGetValue(HistoryItemId, out var clipId))
            {
                throw new ArgumentException("Failed to parse clip id!");
            }

            var audioType = outputFormat.GetAudioType();
            var extension = audioType switch
            {
                AudioType.MPEG => "mp3",
                AudioType.OGGVORBIS => "ogg",
                _ => throw new ArgumentOutOfRangeException($"Unsupported {nameof(AudioType)}: {audioType}")
            };

            var cachedPath = $"{downloadDirectory}/{clipId}.{extension}";

            if (!File.Exists(cachedPath))
            {
                switch (audioType)
                {
                    case AudioType.MPEG:
                        await File.WriteAllBytesAsync(cachedPath, response.Data, cancellationToken).ConfigureAwait(false);
                        break;
                    case AudioType.OGGVORBIS:
                        var pcmData = PCMEncoder.Decode(response.Data, PCMFormatSize.SixteenBit);
                        var oggBytes = await OggEncoder.ConvertToBytesAsync(pcmData, 44100, 1, cancellationToken: cancellationToken).ConfigureAwait(false);
                        await File.WriteAllBytesAsync(cachedPath, oggBytes, cancellationToken: cancellationToken).ConfigureAwait(false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Unsupported {nameof(AudioType)}: {audioType}");
                }
            }

            await Awaiters.UnityMainThread;
            var audioClip = await Rest.DownloadAudioClipAsync($"file://{cachedPath}", audioType, cancellationToken: cancellationToken);
            return new VoiceClip(clipId, text, voice, audioClip, cachedPath);
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
        /// <param name="outputFormat">
        /// Output format of the generated audio.<br/>
        /// Note: Must be PCM format to stream audio in Unity!<br/>
        /// Defaults to <see cref="OutputFormat.PCM_24000"/>.
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
        public async Task<VoiceClip> StreamTextToSpeechAsync(string text, Voice voice, Action<AudioClip> partialClipCallback, VoiceSettings voiceSettings = null, Model model = null, OutputFormat outputFormat = OutputFormat.PCM_24000, int? optimizeStreamingLatency = null, CancellationToken cancellationToken = default)
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

            var frequency = outputFormat switch
            {
                OutputFormat.MP3_44100_64 => throw new InvalidOperationException($"{nameof(outputFormat)} must be a PCM format for streaming!"),
                OutputFormat.MP3_44100_96 => throw new InvalidOperationException($"{nameof(outputFormat)} must be a PCM format for streaming!"),
                OutputFormat.MP3_44100_128 => throw new InvalidOperationException($"{nameof(outputFormat)} must be a PCM format for streaming!"),
                OutputFormat.MP3_44100_192 => throw new InvalidOperationException($"{nameof(outputFormat)} must be a PCM format for streaming!"),
                OutputFormat.PCM_16000 => 16000,
                OutputFormat.PCM_22050 => 22050,
                OutputFormat.PCM_24000 => 24000,
                OutputFormat.PCM_44100 => 44100,
                _ => throw new ArgumentOutOfRangeException(nameof(outputFormat), outputFormat, null)
            };
            var downloadDirectory = await GetCacheDirectoryAsync(voice);
            var defaultVoiceSettings = voiceSettings ?? voice.Settings ?? await client.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken);
            var request = new TextToSpeechRequest(text, model, defaultVoiceSettings);
            var payload = JsonConvert.SerializeObject(request, ElevenLabsClient.JsonSerializationOptions);
            var parameters = new Dictionary<string, string>
            {
                { OutputFormatParameter, outputFormat.ToString().ToLower() }
            };

            if (optimizeStreamingLatency.HasValue)
            {
                parameters.Add(OptimizeStreamingLatencyParameter, optimizeStreamingLatency.Value.ToString());
            }

            var part = 0;
            var response = await Rest.PostAsync(GetUrl($"/{voice.Id}/stream", parameters), payload, StreamCallback, eventChunkSize: 8192, new RestParameters(client.DefaultRequestHeaders), cancellationToken).ConfigureAwait(true);
            response.Validate(EnableDebug);

            if (!response.Headers.TryGetValue(HistoryItemId, out var clipId))
            {
                throw new ArgumentException("Failed to parse clip id!");
            }

            var pcmData = PCMEncoder.Decode(response.Data, PCMFormatSize.SixteenBit);
            var fullClip = AudioClip.Create(clipId, pcmData.Length, 1, frequency, false);
            var oggBytes = await OggEncoder.ConvertToBytesAsync(pcmData, frequency, 1, cancellationToken: cancellationToken).ConfigureAwait(false);
            var cachedPath = $"{downloadDirectory}/{clipId}.ogg";
            await File.WriteAllBytesAsync(cachedPath, oggBytes, cancellationToken: cancellationToken).ConfigureAwait(false);
            await Awaiters.UnityMainThread;
            await Rest.DownloadAudioClipAsync($"file://{cachedPath}", AudioType.OGGVORBIS, cancellationToken: cancellationToken);
            return new VoiceClip(clipId, text, voice, fullClip, cachedPath);

            void StreamCallback(Response partialResponse)
            {
                try
                {
                    if (!partialResponse.Headers.TryGetValue(HistoryItemId, out clipId))
                    {
                        throw new ArgumentException("Failed to parse clip id!");
                    }

                    var chunk = PCMEncoder.Decode(partialResponse.Data, PCMFormatSize.SixteenBit);
                    var audioClip = AudioClip.Create($"{clipId}_{++part}", chunk.Length, 1, 44100, false);

                    if (!audioClip.SetData(chunk, 0))
                    {
                        Debug.LogError("Failed to set pcm data to partial clip.");

                        return;
                    }

                    partialClipCallback.Invoke(audioClip);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
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
