// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using ElevenLabs.Models;
using ElevenLabs.Voices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Utilities.Audio;
using Utilities.Encoding.OggVorbis;
using Utilities.Encoding.Wav;
using Utilities.Extensions;
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
        private const string EnableLoggingParameter = "enable_logging";

        /// <summary>
        /// When set to <c>true</c>, zero retention mode will be used for the request.
        /// This will mean history features are unavailable for this request, including request stitching.
        /// Zero retention mode may only be used by enterprise customers.
        /// </summary>
        public bool? DisableRetention { get; set; }

        public TextToSpeechEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "text-to-speech";

        [Obsolete("use overload with TextToSpeechRequest")]
        public async Task<VoiceClip> TextToSpeechAsync(string text, Voice voice, VoiceSettings voiceSettings = null, Model model = null, OutputFormat outputFormat = OutputFormat.MP3_44100_128, int? optimizeStreamingLatency = null, CancellationToken cancellationToken = default)
            => await TextToSpeechAsync(
                new TextToSpeechRequest(
                    voice,
                    text,
                    Encoding.UTF8,
                    voiceSettings ?? voice.Settings ?? await client.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken),
                    outputFormat,
                    optimizeStreamingLatency,
                    model),
                cancellationToken);

        [Obsolete("use TextToSpeechAsync with VoiceClip partialClipCallback")]
        public async Task<VoiceClip> StreamTextToSpeechAsync(string text, Voice voice, Action<AudioClip> partialClipCallback, VoiceSettings voiceSettings = null, Model model = null, OutputFormat outputFormat = OutputFormat.PCM_24000, int? optimizeStreamingLatency = null, CancellationToken cancellationToken = default)
            => await StreamTextToSpeechAsync(
                new TextToSpeechRequest(
                    voice,
                    text,
                    Encoding.UTF8,
                    voiceSettings ?? voice.Settings ?? await client.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken),
                    outputFormat,
                    optimizeStreamingLatency,
                    model),
                partialClipCallback, cancellationToken);

        [Obsolete("use TextToSpeechAsync with VoiceClip partialClipCallback")]
        public async Task<VoiceClip> StreamTextToSpeechAsync(TextToSpeechRequest request, Action<AudioClip> partialClipCallback, CancellationToken cancellationToken = default)
            => await TextToSpeechAsync(request, voiceClip =>
            {
                partialClipCallback.Invoke(voiceClip.AudioClip);
            }, cancellationToken);

        /// <summary>
        /// Converts text to synthesized speech.
        /// </summary>
        /// <param name="request"><see cref="TextToSpeechRequest"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceClip"/>.</returns>
        public async Task<VoiceClip> TextToSpeechAsync(TextToSpeechRequest request, CancellationToken cancellationToken = default)
        {
            request.VoiceSettings ??= await client.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken);
            var payload = JsonConvert.SerializeObject(request, ElevenLabsClient.JsonSerializationOptions);
            var parameters = CreateRequestParameters(request);
            var endpoint = $"/{request.Voice}";

            if (request.WithTimestamps)
            {
                endpoint += "/with-timestamps";
            }

            var response = await Rest.PostAsync(GetUrl(endpoint, parameters), payload, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);

            if (!response.Headers.TryGetValue(HistoryItemId, out var clipId))
            {
                throw new ArgumentException("Failed to parse clip id!");
            }

            NativeArray<byte> audioData;
            TimestampedTranscriptCharacter[] transcriptionCharacters = null;

            if (request.WithTimestamps)
            {
                var transcriptResponse = JsonConvert.DeserializeObject<TranscriptionResponse>(response.Body, ElevenLabsClient.JsonSerializationOptions);
                audioData = NativeArrayExtensions.FromBase64String(transcriptResponse.AudioBase64, Allocator.Persistent);
                transcriptionCharacters = transcriptResponse.Alignment;
            }
            else
            {
                audioData = new NativeArray<byte>(response.Data, Allocator.Persistent);
            }

            var cachedPath = await SaveAudioToCache(audioData, clipId, request.Voice, request.OutputFormat, request.CacheFormat, cancellationToken).ConfigureAwait(true);

            return new VoiceClip(clipId, request.Text, request.Voice, audioData, request.OutputFormat.GetSampleRate(), cachedPath)
            {
                TimestampedTranscriptCharacters = transcriptionCharacters
            };
        }

        /// <summary>
        /// Converts text to synthesized speech.
        /// </summary>
        /// <param name="request"><see cref="TextToSpeechRequest"/>.</param>
        /// <param name="partialClipCallback">Partial <see cref="VoiceClip"/> callback with streaming data.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceClip"/>.</returns>
        [Obsolete("use overload with Func<VoiceClip, Task> partialClipCallback")]
        public async Task<VoiceClip> TextToSpeechAsync(TextToSpeechRequest request, Action<VoiceClip> partialClipCallback, CancellationToken cancellationToken = default)
        {
            return await TextToSpeechAsync(request, async voiceClip =>
            {
                partialClipCallback.Invoke(voiceClip);
                await Task.Yield();
            }, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Converts text to synthesized speech.
        /// </summary>
        /// <param name="request"><see cref="TextToSpeechRequest"/>.</param>
        /// <param name="partialClipCallback">Partial <see cref="VoiceClip"/> callback with streaming data.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceClip"/>.</returns>
        public async Task<VoiceClip> TextToSpeechAsync(TextToSpeechRequest request, Func<VoiceClip, Task> partialClipCallback, CancellationToken cancellationToken = default)
        {
            if (request.OutputFormat is not OutputFormat.PCM_16000 and not OutputFormat.PCM_22050 and not OutputFormat.PCM_24000 and not OutputFormat.PCM_44100)
            {
                Debug.LogWarning($"{nameof(request.OutputFormat)} must be a PCM format! defaulting to 24000");
                request.OutputFormat = OutputFormat.PCM_24000;
            }

            request.VoiceSettings ??= await client.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken);

            var frequency = request.OutputFormat.GetSampleRate();
            var payload = JsonConvert.SerializeObject(request, ElevenLabsClient.JsonSerializationOptions);
            var parameters = CreateRequestParameters(request);
            var endpoint = $"/{request.Voice.Id}/stream";

            var part = 0;
            StringBuilder textBuffer;
            List<byte> accumulatedPCMData = null;
            List<TimestampedTranscriptCharacter> accumulatedTranscriptData = null;
            Action<Response> streamCallback;

            if (request.WithTimestamps)
            {
                endpoint += "/with-timestamps";
                textBuffer = new StringBuilder();
                accumulatedPCMData = new List<byte>();
                accumulatedTranscriptData = new List<TimestampedTranscriptCharacter>();
                streamCallback = TranscriptionStreamCallback;
            }
            else
            {
                streamCallback = StreamCallback;
            }

            var response = await Rest.PostAsync(GetUrl(endpoint, parameters), payload, streamCallback, 8192, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);

            if (!response.Headers.TryGetValue(HistoryItemId, out var clipId))
            {
                throw new ArgumentException("Failed to parse clip id!");
            }

            var audioData = request.WithTimestamps ? accumulatedPCMData!.ToArray() : response.Data;
            var cachedPath = await SaveAudioToCache(new NativeArray<byte>(audioData, Allocator.Persistent), clipId, request.Voice, request.OutputFormat, request.CacheFormat, cancellationToken).ConfigureAwait(true);

            return new VoiceClip(clipId, request.Text, request.Voice, new NativeArray<byte>(audioData, Allocator.Persistent), frequency, cachedPath)
            {
                TimestampedTranscriptCharacters = accumulatedTranscriptData?.ToArray() ?? Array.Empty<TimestampedTranscriptCharacter>()
            };

            async void StreamCallback(Response partialResponse)
            {
                try
                {
                    if (!partialResponse.Headers.TryGetValue(HistoryItemId, out clipId))
                    {
                        throw new ArgumentException("Failed to parse clip id!");
                    }

                    var partialClip = new VoiceClip($"{clipId}_{++part}", request.Text, request.Voice, new NativeArray<byte>(partialResponse.Data, Allocator.Persistent), frequency);

                    try
                    {
                        await partialClipCallback.Invoke(partialClip);
                    }
                    finally
                    {
                        partialClip.Dispose();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            void TranscriptionStreamCallback(Response partialResponse)
            {
                try
                {
                    if (!partialResponse.Headers.TryGetValue(HistoryItemId, out clipId))
                    {
                        throw new ArgumentException("Failed to parse clip id!");
                    }

                    var chunkText = Encoding.UTF8.GetString(partialResponse.Data);
                    textBuffer.Append(chunkText);

                    // Process any complete lines
                    var text = textBuffer.ToString();
                    var lines = text.Split('\n');

                    // Keep the last potentially incomplete line
                    textBuffer.Clear();
                    textBuffer.Append(lines[^1]);

                    // Process all complete lines
                    for (var i = 0; i < lines.Length - 1; i++)
                    {
                        ProcessTranscribedVoiceClip(lines[i].Trim());
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            void ProcessTranscribedVoiceClip(string line)
            {
                if (string.IsNullOrEmpty(line)) { return; }

                try
                {
                    var partialTranscription = JsonConvert.DeserializeObject<TranscriptionResponse>(line, ElevenLabsClient.JsonSerializationOptions);
                    var partialData = NativeArrayExtensions.FromBase64String(partialTranscription.AudioBase64, Allocator.Persistent);
                    var timestampedTranscriptCharacters = (TimestampedTranscriptCharacter[])partialTranscription.Alignment ?? Array.Empty<TimestampedTranscriptCharacter>();
                    var partialClip = new VoiceClip(
                        id: $"{clipId}_{++part}",
                        text: request.Text,
                        voice: request.Voice,
                        clipData: partialData,
                        sampleRate: frequency)
                    {
                        TimestampedTranscriptCharacters = timestampedTranscriptCharacters
                    };
                    try
                    {
                        partialClipCallback.Invoke(partialClip);
                    }
                    finally
                    {
                        accumulatedTranscriptData.AddRange(timestampedTranscriptCharacters);
                        accumulatedPCMData.AddRange(partialData.ToArray());
                        // partialData is disposed by its partialClip owner
                        partialClip.Dispose();
                    }
                }
                catch (JsonReaderException e)
                {
                    Debug.LogWarning($"Failed to parse line as JSON: {e.Message}");
                }
            }
        }

        private Dictionary<string, string> CreateRequestParameters(TextToSpeechRequest request)
        {
            var parameters = new Dictionary<string, string>
            {
                { OutputFormatParameter, request.OutputFormat.ToString().ToLower() }
            };

#pragma warning disable CS0618 // Type or member is obsolete
            if (request.OptimizeStreamingLatency.HasValue)
            {
                parameters.Add(OptimizeStreamingLatencyParameter, request.OptimizeStreamingLatency.Value.ToString());
            }
#pragma warning restore CS0618 // Type or member is obsolete

            if (DisableRetention.HasValue)
            {
                parameters.Add(EnableLoggingParameter, DisableRetention.GetValueOrDefault().ToString().ToLower());
            }

            return parameters;
        }

        private static async Task<string> SaveAudioToCache(NativeArray<byte> audioData, string clipId, Voice voice, OutputFormat outputFormat, CacheFormat cacheFormat, CancellationToken cancellationToken)
        {
#if PLATFORM_WEBGL
            await Task.Yield();
            return null;
#else
            if (cacheFormat == CacheFormat.None)
            {
                return null;
            }

            string extension;
            AudioType audioType;

            if (outputFormat is OutputFormat.MP3_44100_64 or OutputFormat.MP3_44100_96 or OutputFormat.MP3_44100_128 or OutputFormat.MP3_44100_128)
            {
                extension = "mp3";
                audioType = AudioType.MPEG;
            }
            else
            {
                switch (cacheFormat)
                {
                    case CacheFormat.Wav:
                        extension = "wav";
                        audioType = AudioType.WAV;

                        break;
                    case CacheFormat.Ogg:
                        extension = "ogg";
                        audioType = AudioType.OGGVORBIS;

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(cacheFormat), cacheFormat, null);
                }
            }

            await Rest.ValidateCacheDirectoryAsync();

            var downloadDirectory = Rest.DownloadCacheDirectory
                .CreateNewDirectory(nameof(ElevenLabs))
                .CreateNewDirectory(nameof(TextToSpeech))
                .CreateNewDirectory(voice.Id);

            var cachedPath = $"{downloadDirectory}/{clipId}.{extension}";

            if (!File.Exists(cachedPath))
            {
                switch (audioType)
                {
                    case AudioType.MPEG:
                        await File.WriteAllBytesAsync(cachedPath, audioData.ToArray(), cancellationToken).ConfigureAwait(false);
                        break;
                    case AudioType.OGGVORBIS:
                        var pcmData = PCMEncoder.Decode(audioData);

                        try
                        {
                            var oggBytes = await OggEncoder.ConvertToBytesAsync(pcmData.ToArray(), sampleRate: outputFormat.GetSampleRate(), channels: 1, cancellationToken: cancellationToken).ConfigureAwait(false);
                            await File.WriteAllBytesAsync(cachedPath, oggBytes, cancellationToken).ConfigureAwait(false);
                        }
                        finally
                        {
                            pcmData.Dispose();
                        }

                        break;
                    case AudioType.WAV:
                        await WavEncoder.WriteToFileAsync(cachedPath, audioData, sampleRate: outputFormat.GetSampleRate(), channels: 1, cancellationToken: cancellationToken).ConfigureAwait(false);

                        break;
                }
            }

            return cachedPath;
#endif // PLATFORM_WEBGL
        }

    }
}
