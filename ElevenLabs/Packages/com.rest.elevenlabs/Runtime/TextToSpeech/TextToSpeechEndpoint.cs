// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Utilities.Async;
using Utilities.WebRequestRest;
using Utilities.WebRequestRest.Interfaces;
using Utilities.WebSockets;

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
        /// Converts text to synthesized speech.
        /// </summary>
        /// <param name="request"><see cref="TextToSpeechRequest"/>.</param>
        /// <param name="partialClipCallback">Partial <see cref="VoiceClip"/> callback with streaming data.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceClip"/>.</returns>
        public async Task<VoiceClip> TextToSpeechAsync(TextToSpeechRequest request, Func<VoiceClip, Task> partialClipCallback = null, CancellationToken cancellationToken = default)
        {
            request.VoiceSettings ??= await client.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken);
            var payload = JsonConvert.SerializeObject(request, ElevenLabsClient.JsonSerializationOptions);
            var parameters = new Dictionary<string, string>
            {
                { OutputFormatParameter, request.OutputFormat.ToString().ToLower() }
            };

            var endpoint = $"/{request.Voice.Id}";

            if (partialClipCallback != null)
            {
                if (request.OutputFormat is not OutputFormat.PCM_16000 and not OutputFormat.PCM_22050 and not OutputFormat.PCM_24000 and not OutputFormat.PCM_44100)
                {
                    Debug.LogWarning($"{nameof(request.OutputFormat)} must be a PCM format! Was {request.OutputFormat}, defaulting to 24000");
                    request.OutputFormat = OutputFormat.PCM_24000;
                }

                endpoint += "/stream";
            }

            var frequency = request.OutputFormat.GetSampleRate();

            var part = 0;
            string clipId;
            StringBuilder textBuffer;
            NativeQueue<byte>? accumulatedPCMData = null;
            List<TimestampedTranscriptCharacter> transcriptionCharacters = null;
            Action<Response> streamCallback = null;

            if (request.WithTimestamps)
            {
                endpoint += "/with-timestamps";

                if (partialClipCallback != null)
                {
                    textBuffer = new StringBuilder();
                    accumulatedPCMData = new NativeQueue<byte>(Allocator.Persistent);
                    transcriptionCharacters = new List<TimestampedTranscriptCharacter>();
                    streamCallback = TranscriptionStreamCallback;
                }
            }
            else
            {
                if (partialClipCallback != null)
                {
                    streamCallback = StreamCallback;
                }
            }

            try
            {
                Response response;

                if (partialClipCallback != null)
                {
                    response = await Rest.PostAsync(GetUrl(endpoint, parameters), payload, streamCallback, 8192, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
                }
                else
                {
                    response = await Rest.PostAsync(GetUrl(endpoint, parameters), payload, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
                }

                response.Validate(EnableDebug);

                if (!response.Headers.TryGetValue(HistoryItemId, out clipId))
                {
                    throw new ArgumentException("Failed to parse clip id!");
                }

                NativeArray<byte> audioData;

                if (request.WithTimestamps)
                {
                    if (accumulatedPCMData.HasValue)
                    {
                        audioData = accumulatedPCMData.Value.ToArray(Allocator.Persistent);
                    }
                    else
                    {
                        var transcriptResponse = JsonConvert.DeserializeObject<TranscriptionResponse>(response.Body, ElevenLabsClient.JsonSerializationOptions);
                        audioData = Utilities.Extensions.NativeArrayExtensions.FromBase64String(transcriptResponse.AudioBase64, Allocator.Persistent);
                        transcriptionCharacters = transcriptResponse.Alignment;
                    }
                }
                else
                {
                    audioData = new NativeArray<byte>(response.Data, Allocator.Persistent);
                }

                var voiceClip = new VoiceClip(clipId, request.Text, request.Voice, audioData, frequency)
                {
                    TimestampedTranscriptCharacters = transcriptionCharacters?.ToArray() ?? Array.Empty<TimestampedTranscriptCharacter>()
                };

                try
                {
                    await voiceClip.SaveAudioToCacheAsync(request.OutputFormat, request.CacheFormat, cancellationToken);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                return voiceClip;
            }
            finally
            {
                accumulatedPCMData?.Dispose();
            }

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

            async void TranscriptionStreamCallback(Response partialResponse)
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
                        await ProcessTranscribedVoiceClip(lines[i].Trim());
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            async Task ProcessTranscribedVoiceClip(string line)
            {
                if (string.IsNullOrEmpty(line)) { return; }

                try
                {
                    var partialTranscription = JsonConvert.DeserializeObject<TranscriptionResponse>(line, ElevenLabsClient.JsonSerializationOptions);
                    var partialData = Utilities.Extensions.NativeArrayExtensions.FromBase64String(partialTranscription.AudioBase64, Allocator.Persistent);
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
                        await partialClipCallback.Invoke(partialClip).ConfigureAwait(false);
                    }
                    finally
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        transcriptionCharacters?.AddRange(timestampedTranscriptCharacters);
                        var length = partialData.Length;

                        for (var i = 0; i < length; i++)
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            accumulatedPCMData.Value.Enqueue(partialData[i]);
                        }

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

        /// <summary>
        /// Create a websocket <see cref="TextToSpeechSession"/>.
        /// </summary>
        /// <param name="configuration">The configuration for the text-to-speech session.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="TextToSpeechSession"/>.</returns>
        public async Task<TextToSpeechSession> CreateTextToSpeechSessionAsync(TextToSpeechSessionConfiguration configuration, CancellationToken cancellationToken = default)
        {
            if (configuration == null)
            {
                throw new NullReferenceException(nameof(configuration));
            }

            var endpoint = GetWebsocketUri($"{configuration.Voice.Id}/stream-input", configuration.ToQueryParams());
            var websocket = new WebSocket(endpoint, client.DefaultRequestHeaders);
            var session = new TextToSpeechSession(websocket, EnableDebug);
            var initializeSessionTcs = new TaskCompletionSource<bool>();

            try
            {
                session.OnEventReceived += OnEventReceived;
                session.OnError += OnError;
                await session.ConnectAsync(cancellationToken).ConfigureAwait(false);
                await initializeSessionTcs.Task.WithCancellation(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                session.OnError -= OnError;
                session.OnEventReceived -= OnEventReceived;
            }

            return session;

            void OnError(Exception e)
                => initializeSessionTcs.SetException(e);

            void OnEventReceived(IServerSentEvent @event)
            {
                try
                {
                    switch (@event)
                    {
                        default:
                            initializeSessionTcs.TrySetResult(true);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    initializeSessionTcs.TrySetException(e);
                }
            }
        }
    }
}
