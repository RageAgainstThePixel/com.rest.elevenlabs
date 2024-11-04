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
using Newtonsoft.Json.Linq;
using UnityEngine;
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
        private const string AlignmentToken = "alignment";
        private const string CharactersToken = "characters";
        private const string CharacterStartTimesToken = "character_start_times_seconds";
        private const string CharacterEndTimesToken = "character_end_times_seconds";

        public TextToSpeechEndpoint(ElevenLabsClient client) : base(client)
        {
        }

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
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceClip"/>.</returns>
        public async Task<VoiceClip> TextToSpeechAsync(string text, Voice voice, VoiceSettings voiceSettings = null, Model model = null, OutputFormat outputFormat = OutputFormat.MP3_44100_128, int? optimizeStreamingLatency = null, CancellationToken cancellationToken = default)
        {
            return await TextToSpeechAsync(new TextToSpeechRequest(voice, text, Encoding.UTF8, voiceSettings ?? voice.Settings ?? await client.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken), outputFormat, optimizeStreamingLatency, model), cancellationToken);
        }

        /// <summary>
        /// Converts text into speech using a voice of your choice and returns audio.
        /// </summary>
        /// <param name="request"><see cref="TextToSpeechRequest"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="VoiceClip"/>.</returns>
        public async Task<VoiceClip> TextToSpeechAsync(TextToSpeechRequest request, CancellationToken cancellationToken = default)
        {
            var payload = JsonConvert.SerializeObject(request, ElevenLabsClient.JsonSerializationOptions);
            var parameters = CreateRequestParameters(request);

            var endpoint = $"/{request.Voice}";

            var response = await Rest.PostAsync(GetUrl(endpoint, parameters), payload, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);

            if (!response.Headers.TryGetValue(HistoryItemId, out var clipId))
            {
                throw new ArgumentException("Failed to parse clip id!");
            }

            var cachedPath = await SaveAudioToCache(response.Data, clipId, request.Voice, request.OutputFormat, cancellationToken);

            return await CreateVoiceClipFromFile(clipId, request.Text, request.Voice, cachedPath, request.OutputFormat, cancellationToken);
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
        /// Optional, Callback to enable streaming audio as it comes in.<br/>
        /// Returns partial <see cref="VoiceClip"/>.
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
            return await StreamTextToSpeechAsync(new TextToSpeechRequest(voice, text, Encoding.UTF8, voiceSettings ?? voice.Settings ?? await client.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken), outputFormat, optimizeStreamingLatency, model), partialClipCallback, cancellationToken);
        }

        /// <summary>
        /// Converts text into speech using a voice of your choice and returns audio as an audio stream.
        /// </summary>
        /// <param name="request"><see cref="TextToSpeechRequest"/>.</param>
        /// <param name="partialClipCallback">
        /// Optional, Callback to enable streaming audio as it comes in.<br/>
        /// Returns partial <see cref="VoiceClip"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional, <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>Downloaded clip path, and the loaded audio clip.</returns>
        public async Task<VoiceClip> StreamTextToSpeechAsync(TextToSpeechRequest request, Action<AudioClip> partialClipCallback, CancellationToken cancellationToken = default)
        {
            ValidateStreamingFormat(request.OutputFormat);
            var frequency = GetFrequencyForPCMFormat(request.OutputFormat);

            var payload = JsonConvert.SerializeObject(request, ElevenLabsClient.JsonSerializationOptions);
            var parameters = CreateRequestParameters(request);

            var endpoint = $"/{request.Voice.Id}/stream";

            var part = 0;

            var response = await Rest.PostAsync(GetUrl(endpoint, parameters), payload, StreamCallback, 8192, new RestParameters(client.DefaultRequestHeaders), cancellationToken).ConfigureAwait(true);
            response.Validate(EnableDebug);

            if (!response.Headers.TryGetValue(HistoryItemId, out var clipId))
            {
                throw new ArgumentException("Failed to parse clip id!");
            }

            var pcmData = PCMEncoder.Decode(response.Data);
            var downloadDirectory = await GetCacheDirectoryAsync(request.Voice);
            var cachedPath = $"{downloadDirectory}/{clipId}.ogg";
            var oggBytes = await OggEncoder.ConvertToBytesAsync(pcmData, frequency, 1, cancellationToken: cancellationToken);
            await File.WriteAllBytesAsync(cachedPath, oggBytes, cancellationToken);
            var fullClip = await Rest.DownloadAudioClipAsync($"file://{cachedPath}", AudioType.OGGVORBIS, parameters: new RestParameters(debug: EnableDebug), compressed: false, streamingAudio: true, cancellationToken: cancellationToken);

            return new VoiceClip(clipId, request.Text, request.Voice, fullClip, cachedPath);

            void StreamCallback(Response partialResponse)
            {
                try
                {
                    if (!partialResponse.Headers.TryGetValue(HistoryItemId, out clipId))
                    {
                        throw new ArgumentException("Failed to parse clip id!");
                    }

                    var chunk = PCMEncoder.Decode(partialResponse.Data);
                    var audioClip = AudioClip.Create($"{clipId}_{++part}", chunk.Length, 1, frequency, false);

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


        /// <summary>
        /// Converts text into speech using a voice of your choice and returns audio and a list of timestamped characters.
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
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A tuple containing the <see cref="VoiceClip"/> and an array of <see cref="TimestampedTranscriptCharacter"/>.</returns>
        public async Task<TranscribedVoiceClip> TextToSpeechWithTimestampsAsync(string text, Voice voice, VoiceSettings voiceSettings = null, Model model = null, OutputFormat outputFormat = OutputFormat.MP3_44100_128, int? optimizeStreamingLatency = null, CancellationToken cancellationToken = default)
        {
            return await TextToSpeechWithTimestampsAsync(new TextToSpeechRequest(voice, text, Encoding.UTF8, voiceSettings ?? voice.Settings ?? await client.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken), outputFormat, optimizeStreamingLatency, model), cancellationToken);
        }

        /// <summary>
        /// Converts text into speech using a voice of your choice and returns audio.
        /// </summary>
        /// <param name="request"><see cref="TextToSpeechRequest"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A tuple containing the <see cref="VoiceClip"/> and an array of <see cref="TimestampedTranscriptCharacter"/>.</returns>
        public async Task<TranscribedVoiceClip> TextToSpeechWithTimestampsAsync(TextToSpeechRequest request, CancellationToken cancellationToken = default)
        {
            var payload = JsonConvert.SerializeObject(request, ElevenLabsClient.JsonSerializationOptions);
            var parameters = CreateRequestParameters(request);

            var endpoint = $"/{request.Voice.Id}/with-timestamps";

            var response = await Rest.PostAsync(GetUrl(endpoint, parameters), payload, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);

            if (!response.Headers.TryGetValue(HistoryItemId, out var clipId))
            {
                throw new ArgumentException("Failed to parse clip id!");
            }

            var responseJson = Encoding.UTF8.GetString(response.Data);
            var jsonObject = JObject.Parse(responseJson);

            var audioBase64 = jsonObject["audio_base64"].ToString();
            var audioBytes = Convert.FromBase64String(audioBase64);

            var audioType = request.OutputFormat.GetAudioType();
            var cachedPath = await SaveAudioToCache(audioBytes, clipId, request.Voice, request.OutputFormat, cancellationToken);

            await Awaiters.UnityMainThread;
            var audioClip = await Rest.DownloadAudioClipAsync($"file://{cachedPath}", audioType, parameters: new RestParameters(debug: EnableDebug), cancellationToken: cancellationToken);
            var voiceClip = new VoiceClip(clipId, request.Text, request.Voice, audioClip, cachedPath);

            var timestampedTranscriptCharacters = ExtractTranscriptCharacters(jsonObject);

            return new TranscribedVoiceClip(voiceClip, timestampedTranscriptCharacters);
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
        /// Optional, Callback to enable streaming audio as it comes in.<br/>
        /// Returns partial <see cref="VoiceClip"/>.
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
        public async Task<TranscribedVoiceClip> StreamTextToSpeechWithTimestampsAsync(string text, Voice voice, Action<PartialTranscribedAudioClip> partialClipCallback, VoiceSettings voiceSettings = null, Model model = null, OutputFormat outputFormat = OutputFormat.PCM_24000, int? optimizeStreamingLatency = null, CancellationToken cancellationToken = default)
        {
            return await StreamTextToSpeechWithTimestampsAsync(new TextToSpeechRequest(voice, text, Encoding.UTF8, voiceSettings ?? voice.Settings ?? await client.VoicesEndpoint.GetDefaultVoiceSettingsAsync(cancellationToken), outputFormat, optimizeStreamingLatency, model), partialClipCallback, cancellationToken);
        }

        /// <summary>
        /// Converts text into speech using a voice of your choice and returns audio as an audio stream.
        /// </summary>
        /// <param name="request"><see cref="TextToSpeechRequest"/>.</param>
        /// <param name="partialClipCallback">
        /// Optional, Callback to enable streaming audio as it comes in.<br/>
        /// Returns partial <see cref="VoiceClip"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional, <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>Downloaded clip path, and the loaded audio clip.</returns>
        public async Task<TranscribedVoiceClip> StreamTextToSpeechWithTimestampsAsync(TextToSpeechRequest request, Action<PartialTranscribedAudioClip> partialClipCallback, CancellationToken cancellationToken = default)
        {
            ValidateStreamingFormat(request.OutputFormat);
            var frequency = GetFrequencyForPCMFormat(request.OutputFormat);

            var payload = JsonConvert.SerializeObject(request, ElevenLabsClient.JsonSerializationOptions);
            var parameters = CreateRequestParameters(request);

            var endpoint = $"/{request.Voice.Id}/stream/with-timestamps";

            var part = 0;
            var textBuffer = new StringBuilder();
            var accumulatedPCMData = new List<float>();
            var accumulatedTranscriptData = new List<TimestampedTranscriptCharacter>();

            var response = await Rest.PostAsync(GetUrl(endpoint, parameters), payload, StreamCallback, 8192, new RestParameters(client.DefaultRequestHeaders), cancellationToken).ConfigureAwait(true);
            response.Validate(EnableDebug);

            if (!response.Headers.TryGetValue(HistoryItemId, out var clipId))
            {
                throw new ArgumentException("Failed to parse clip id!");
            }

            // The response in this case is also an array of JSON objects, exactly matching what was already sent.
            // Rather than parsing the full response we use the data we've been streaming already.
            var pcmData = accumulatedPCMData.ToArray();
            var downloadDirectory = await GetCacheDirectoryAsync(request.Voice);
            var cachedPath = $"{downloadDirectory}/{clipId}.ogg";
            var oggBytes = await OggEncoder.ConvertToBytesAsync(pcmData, frequency, 1, cancellationToken: cancellationToken);
            await File.WriteAllBytesAsync(cachedPath, oggBytes, cancellationToken);
            var fullClip = await Rest.DownloadAudioClipAsync($"file://{cachedPath}", AudioType.OGGVORBIS, parameters: new RestParameters(debug: EnableDebug), compressed: false, streamingAudio: true, cancellationToken: cancellationToken);
            var voiceClip = new VoiceClip(clipId, request.Text, request.Voice, fullClip, cachedPath);

            return new TranscribedVoiceClip(voiceClip, accumulatedTranscriptData.ToArray());

            void StreamCallback(Response partialResponse)
            {
                try
                {
                    if (!partialResponse.Headers.TryGetValue(HistoryItemId, out clipId))
                    {
                        throw new ArgumentException("Failed to parse clip id!");
                    }

                    var chunkText = Encoding.UTF8.GetString(partialResponse.Data);
                    textBuffer.Append(chunkText);
                    TryFlushAccumulatedTextLines();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            void TryFlushAccumulatedTextLines()
            {
                // Process any complete lines
                var text = textBuffer.ToString();
                var lines = text.Split('\n');

                // Keep the last potentially incomplete line
                textBuffer.Clear();
                textBuffer.Append(lines[^1]);

                // Process all complete lines
                for (var i = 0; i < lines.Length - 1; i++)
                {
                    HandleResponseJsonLine(lines[i]);
                }

                void HandleResponseJsonLine(string line)
                {
                    line = line.Trim();

                    if (string.IsNullOrEmpty(line))
                    {
                        return;
                    }

                    try
                    {
                        var responseJson = JObject.Parse(line);

                        var audioBase64 = responseJson["audio_base64"].ToString();
                        var audioBytes = Convert.FromBase64String(audioBase64);

                        var pcmData = PCMEncoder.Decode(audioBytes, PCMFormatSize.SixteenBit);
                        var audioClip = AudioClip.Create($"{clipId}_{++part}", pcmData.Length, 1, frequency, false);

                        if (!audioClip.SetData(pcmData, 0))
                        {
                            Debug.LogError("Failed to set PCM data to partial clip.");

                            return;
                        }

                        var timestampedTranscriptCharacters = ExtractTranscriptCharacters(responseJson);

                        partialClipCallback.Invoke(new PartialTranscribedAudioClip(audioClip, timestampedTranscriptCharacters));

                        accumulatedPCMData.AddRange(pcmData);
                        accumulatedTranscriptData.AddRange(timestampedTranscriptCharacters);
                    }
                    catch (JsonReaderException e)
                    {
                        Debug.LogWarning($"Failed to parse line as JSON: {e.Message}");
                    }
                }
            }
        }

        private static TimestampedTranscriptCharacter[] ExtractTranscriptCharacters(JObject responseJson)
        {
            var timestampedTranscriptCharacters = Array.Empty<TimestampedTranscriptCharacter>();
            var alignmentToken = responseJson.SelectToken(AlignmentToken);

            if (alignmentToken != null)
            {
                var characterArray = alignmentToken.SelectToken(CharactersToken)?.ToObject<string[]>() ?? Array.Empty<string>();
                var startTimeArray = alignmentToken.SelectToken(CharacterStartTimesToken)?.ToObject<double[]>() ?? Array.Empty<double>();
                var endTimeArray = alignmentToken.SelectToken(CharacterEndTimesToken)?.ToObject<double[]>() ?? Array.Empty<double>();

                if (characterArray.Length != startTimeArray.Length || startTimeArray.Length != endTimeArray.Length)
                {
                    throw new ArgumentException("Characters, start times, and end times must have the same length");
                }

                timestampedTranscriptCharacters = new TimestampedTranscriptCharacter[characterArray.Length];

                for (var c = 0; c < characterArray.Length; c++)
                {
                    timestampedTranscriptCharacters[c] = new TimestampedTranscriptCharacter(characterArray[c], startTimeArray[c], endTimeArray[c]);
                }
            }

            return timestampedTranscriptCharacters;
        }


        private Dictionary<string, string> CreateRequestParameters(TextToSpeechRequest request)
        {
            var parameters = new Dictionary<string, string>
            {
                {OutputFormatParameter, request.OutputFormat.ToString().ToLower()}
            };

            if (request.OptimizeStreamingLatency.HasValue)
            {
                parameters.Add(OptimizeStreamingLatencyParameter, request.OptimizeStreamingLatency.Value.ToString());
            }

            return parameters;
        }


        private static async Task<string> GetCacheDirectoryAsync(Voice voice)
        {
            await Rest.ValidateCacheDirectoryAsync();

            return Rest.DownloadCacheDirectory
                .CreateNewDirectory(nameof(ElevenLabs))
                .CreateNewDirectory(nameof(TextToSpeech))
                .CreateNewDirectory(voice.Id);
        }

        private async Task<string> SaveAudioToCache(byte[] audioBytes, string clipId, Voice voice, OutputFormat outputFormat, CancellationToken cancellationToken)
        {
            var audioType = outputFormat.GetAudioType();
            ValidateAudioType(audioType);

            var extension = GetFileExtension(audioType);
            var downloadDirectory = await GetCacheDirectoryAsync(voice);
            var cachedPath = $"{downloadDirectory}/{clipId}.{extension}";

            if (!File.Exists(cachedPath))
            {
                switch (audioType)
                {
                    case AudioType.MPEG:
                        await File.WriteAllBytesAsync(cachedPath, audioBytes, cancellationToken).ConfigureAwait(false);

                        break;
                    case AudioType.OGGVORBIS:
                        var pcmData = PCMEncoder.Decode(audioBytes, PCMFormatSize.SixteenBit);
                        var frequency = GetFrequencyForPCMFormat(outputFormat);
                        var oggBytes = await OggEncoder.ConvertToBytesAsync(pcmData, frequency, 1, cancellationToken: cancellationToken).ConfigureAwait(false);
                        await File.WriteAllBytesAsync(cachedPath, oggBytes, cancellationToken).ConfigureAwait(false);

                        break;
                }
            }

            return cachedPath;
        }

        private async Task<VoiceClip> CreateVoiceClipFromFile(string clipId, string text, Voice voice, string cachedPath, OutputFormat outputFormat, CancellationToken cancellationToken)
        {
            await Awaiters.UnityMainThread;

            var audioClip = await Rest.DownloadAudioClipAsync(
                $"file://{cachedPath}",
                outputFormat.GetAudioType(),
                parameters: new RestParameters(debug: EnableDebug),
                cancellationToken: cancellationToken);

            return new VoiceClip(clipId, text, voice, audioClip, cachedPath);
        }


        private void ValidateAudioType(AudioType audioType)
        {
            if (audioType is not AudioType.MPEG and not AudioType.OGGVORBIS)
            {
                throw new ArgumentOutOfRangeException($"Unsupported {nameof(AudioType)}: {audioType}");
            }
        }

        private void ValidateStreamingFormat(OutputFormat format)
        {
            var isInvalidFormat = format switch
            {
                OutputFormat.MP3_44100_64 => true,
                OutputFormat.MP3_44100_96 => true,
                OutputFormat.MP3_44100_128 => true,
                OutputFormat.MP3_44100_192 => true,
                _ => false
            };

            if (isInvalidFormat)
            {
                throw new InvalidOperationException($"{nameof(format)} must be a PCM format for streaming!");
            }
        }

        private string GetFileExtension(AudioType audioType)
        {
            return audioType switch
            {
                AudioType.MPEG => "mp3",
                AudioType.OGGVORBIS => "ogg",
                _ => throw new ArgumentOutOfRangeException($"Unsupported {nameof(AudioType)}: {audioType}")
            };
        }

        private int GetFrequencyForPCMFormat(OutputFormat format)
        {
            return format switch
            {
                OutputFormat.PCM_16000 => 16000,
                OutputFormat.PCM_22050 => 22050,
                OutputFormat.PCM_24000 => 24000,
                OutputFormat.PCM_44100 => 44100,
                _ => throw new ArgumentException($"{nameof(GetFrequencyForPCMFormat)} requires a PCM format!", nameof(format), null)
            };
        }
    }
}
