// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.WebRequestRest;
using Debug = UnityEngine.Debug;

namespace ElevenLabs.Dubbing
{
    public class DubbingEndpoint : ElevenLabsBaseEndPoint
    {
        public DubbingEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "dubbing";

        /// <summary>
        /// Dubs provided audio or video file into given language.
        /// </summary>
        /// <param name="request">The <see cref="DubbingRequest"/> containing dubbing configuration and files.</param>
        /// <param name="progress"><see cref="IProgress{DubbingProjectMetadata}"/> progress callback.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <param name="maxRetries">Optional, number of retry attempts when polling.</param>
        /// <param name="pollingInterval">Optional, <see cref="TimeSpan"/> between making requests.</param>
        /// <returns><see cref="DubbingProjectMetadata"/>.</returns>
        public async Task<DubbingProjectMetadata> DubAsync(DubbingRequest request, int? maxRetries = null, TimeSpan? pollingInterval = null, IProgress<DubbingProjectMetadata> progress = null, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var payload = new WWWForm();

            try
            {
                if (request.Files != null)
                {
                    foreach (var dub in request.Files)
                    {
                        using var audioData = new MemoryStream();
                        await dub.Stream.CopyToAsync(audioData, cancellationToken);
                        payload.AddBinaryData("file", audioData.ToArray(), dub.Name, dub.MediaType);
                    }
                }

                if (!string.IsNullOrEmpty(request.ProjectName))
                {
                    payload.AddField("name", request.ProjectName);
                }

                if (request.SourceUrl != null)
                {
                    payload.AddField("source_url", request.SourceUrl.ToString());
                }

                if (!string.IsNullOrEmpty(request.SourceLanguage))
                {
                    payload.AddField("source_lang", request.SourceLanguage);
                }

                if (!string.IsNullOrEmpty(request.TargetLanguage))
                {
                    payload.AddField("target_lang", request.TargetLanguage);
                }

                if (request.NumberOfSpeakers.HasValue)
                {
                    payload.AddField("num_speakers", request.NumberOfSpeakers.Value.ToString(CultureInfo.InvariantCulture));
                }

                if (request.Watermark.HasValue)
                {
                    payload.AddField("watermark", request.Watermark.Value.ToString());
                }

                if (request.StartTime.HasValue)
                {
                    payload.AddField("start_time", request.StartTime.Value.ToString(CultureInfo.InvariantCulture));
                }

                if (request.EndTime.HasValue)
                {
                    payload.AddField("end_time", request.EndTime.Value.ToString(CultureInfo.InvariantCulture));
                }

                if (request.HighestResolution.HasValue)
                {
                    payload.AddField("highest_resolution", request.HighestResolution.Value.ToString());
                }

                if (request.DropBackgroundAudio.HasValue)
                {
                    payload.AddField("drop_background_audio", request.DropBackgroundAudio.ToString().ToLower());
                }

                if (request.UseProfanityFilter.HasValue)
                {
                    payload.AddField("use_profanity_filter", request.UseProfanityFilter.ToString().ToLower());
                }
            }
            finally
            {
                request.Dispose();
            }

            var response = await Rest.PostAsync(GetUrl(), payload, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            var dubResponse = JsonConvert.DeserializeObject<DubbingResponse>(response.Body, ElevenLabsClient.JsonSerializationOptions);
            return await WaitForDubbingCompletionAsync(dubResponse, maxRetries ?? 60, pollingInterval ?? TimeSpan.FromSeconds(dubResponse.ExpectedDuration), pollingInterval == null, progress, cancellationToken);
        }

        private async Task<DubbingProjectMetadata> WaitForDubbingCompletionAsync(DubbingResponse dubbingResponse, int maxRetries, TimeSpan pollingInterval, bool adjustInterval, IProgress<DubbingProjectMetadata> progress = null, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            for (var i = 1; i < maxRetries + 1; i++)
            {
                var metadata = await GetDubbingProjectMetadataAsync(dubbingResponse, cancellationToken).ConfigureAwait(false);
                metadata.ExpectedDurationSeconds = dubbingResponse.ExpectedDuration;

                if (metadata.Status.Equals("dubbed", StringComparison.Ordinal))
                {
                    stopwatch.Stop();
                    metadata.TimeCompleted = stopwatch.Elapsed;
                    progress?.Report(metadata);
                    return metadata;
                }

                progress?.Report(metadata);

                if (metadata.Status.Equals("dubbing", StringComparison.Ordinal))
                {
                    if (adjustInterval && pollingInterval.TotalSeconds > 0.5f)
                    {
                        pollingInterval = TimeSpan.FromSeconds(dubbingResponse.ExpectedDuration / Math.Pow(2, i));
                    }

                    if (EnableDebug)
                    {
                        Debug.Log($"Dubbing for {dubbingResponse.DubbingId} in progress... Will check status again in {pollingInterval.TotalSeconds} seconds.");
                    }

                    await Task.Delay(pollingInterval, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    throw new Exception($"Dubbing for {dubbingResponse.DubbingId} failed: {metadata.Error}");
                }
            }

            throw new TimeoutException($"Dubbing for {dubbingResponse.DubbingId} timed out or exceeded expected duration.");
        }

        /// <summary>
        /// Returns metadata about a dubbing project, including whether it’s still in progress or not.
        /// </summary>
        /// <param name="dubbingId">Dubbing project id.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="DubbingProjectMetadata"/>.</returns>
        public async Task<DubbingProjectMetadata> GetDubbingProjectMetadataAsync(string dubbingId, CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl($"/{dubbingId}"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<DubbingProjectMetadata>(response.Body, ElevenLabsClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Returns transcript for the dub in the specified format (SRT or WebVTT).
        /// </summary>
        /// <param name="dubbingId">Dubbing project id.</param>
        /// <param name="languageCode">The language code of the transcript.</param>
        /// <param name="formatType">Optional. The format type of the transcript file, either <see cref="DubbingFormat.Srt"/> or <see cref="DubbingFormat.WebVtt"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>
        /// A string containing the transcript content in the specified format.
        /// </returns>
        public async Task<string> GetTranscriptForDubAsync(string dubbingId, string languageCode, DubbingFormat formatType = DubbingFormat.Srt, CancellationToken cancellationToken = default)
        {
            var @params = new Dictionary<string, string> { { "format_type", formatType.ToString().ToLower() } };
            var response = await Rest.GetAsync(GetUrl($"/{dubbingId}/transcript/{languageCode}", @params), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return response.Body;
        }

        /// <summary>
        /// Returns dubbed file as an <see cref="AudioClip"/>.
        /// </summary>
        /// <param name="dubbingId">Dubbing project id.</param>
        /// <param name="languageCode">The language code of the transcript.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>Path to downloaded file.</returns>
        public async Task<string> GetDubbedFileAsync(string dubbingId, string languageCode, CancellationToken cancellationToken = default)
        {
            var result = await Rest.GetAsync(GetUrl($"/{dubbingId}/audio/{languageCode}"), parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken: cancellationToken);
            result.Validate(EnableDebug);
            var cacheDir = await GetCacheDirectoryAsync();
            var mimeType = result.Headers["Content-Type"];
            var extension = mimeType switch
            {
                "video/mp4" => ".mp4",
                "audio/mpeg" => ".mp3",
                _ => throw new NotSupportedException($"Unsupported mime type: {mimeType}")
            };
            var fileName = $"{dubbingId}_{languageCode}{extension}";
            var filePath = Path.Combine(cacheDir, fileName);
            await File.WriteAllBytesAsync(filePath, result.Data, cancellationToken).ConfigureAwait(true);
            return filePath;
        }

        /// <summary>
        /// Deletes a dubbing project.
        /// </summary>
        /// <param name="dubbingId">Dubbing project id.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        public async Task DeleteDubbingProjectAsync(string dubbingId, CancellationToken cancellationToken = default)
        {
            var response = await Rest.DeleteAsync(GetUrl($"/{dubbingId}"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
        }

        private static async Task<string> GetCacheDirectoryAsync()
        {
            await Rest.ValidateCacheDirectoryAsync();
            return Rest.DownloadCacheDirectory
                .CreateNewDirectory(nameof(ElevenLabs))
                .CreateNewDirectory(nameof(Dubbing));
        }
    }
}
