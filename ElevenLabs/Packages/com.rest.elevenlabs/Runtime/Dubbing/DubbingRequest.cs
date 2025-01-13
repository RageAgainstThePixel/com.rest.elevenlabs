// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Utilities.Encoding.Wav;

namespace ElevenLabs.Dubbing
{
    public sealed class DubbingRequest : IDisposable
    {
        public DubbingRequest(
            string filePath,
            string targetLanguage,
            string sourceLanguage = null,
            int? numberOfSpeakers = null,
            bool? watermark = null,
            int? startTime = null,
            int? endTime = null,
            bool? highestResolution = null,
            bool? dropBackgroundAudio = null,
            bool? useProfanityFilter = null,
            string projectName = null)
            : this(new[] { filePath }, targetLanguage, sourceLanguage, numberOfSpeakers, watermark, startTime, endTime, highestResolution, dropBackgroundAudio, useProfanityFilter, projectName)
        {
        }

        public DubbingRequest(
            IEnumerable<string> filePaths,
            string targetLanguage,
            string sourceLanguage = null,
            int? numberOfSpeakers = null,
            bool? watermark = null,
            int? startTime = null,
            int? endTime = null,
            bool? highestResolution = null,
            bool? dropBackgroundAudio = null,
            bool? useProfanityFilter = null,
            string projectName = null)
            : this(targetLanguage, null, null, filePaths, sourceLanguage, numberOfSpeakers, watermark, startTime, endTime, highestResolution, dropBackgroundAudio, useProfanityFilter, projectName)
        {
        }

        public DubbingRequest(
            Uri sourceUrl,
            string targetLanguage,
            string sourceLanguage = null,
            int? numberOfSpeakers = null,
            bool? watermark = null,
            int? startTime = null,
            int? endTime = null,
            bool? highestResolution = null,
            bool? dropBackgroundAudio = null,
            bool? useProfanityFilter = null,
            string projectName = null)
            : this(targetLanguage, sourceUrl, null, null, sourceLanguage, numberOfSpeakers, watermark, startTime, endTime, highestResolution, dropBackgroundAudio, useProfanityFilter, projectName)
        {
        }

        public DubbingRequest(AudioClip audioClip, string targetLanguage, string sourceLanguage = null, int? numberOfSpeakers = null, bool? watermark = null, int? startTime = null, int? endTime = null, bool? highestResolution = null, bool? dropBackgroundAudio = null, bool? useProfanityFilter = null, string projectName = null)
            : this(new[] { audioClip }, targetLanguage, sourceLanguage, numberOfSpeakers, watermark, startTime, endTime, highestResolution, dropBackgroundAudio, useProfanityFilter, projectName)
        {
        }

        public DubbingRequest(
            IEnumerable<AudioClip> audioClips,
            string targetLanguage,
            string sourceLanguage = null,
            int? numberOfSpeakers = null,
            bool? watermark = null,
            int? startTime = null,
            int? endTime = null,
            bool? highestResolution = null,
            bool? dropBackgroundAudio = null,
            bool? useProfanityFilter = null,
            string projectName = null)
        {
            if (audioClips == null)
            {
                throw new MissingReferenceException(nameof(audioClips));
            }

            var clips = audioClips.ToList();

            if (clips.Count == 0)
            {
                throw new ArgumentException("At least one audio clip must be provided.");
            }

            if (string.IsNullOrWhiteSpace(targetLanguage))
            {
                throw new ArgumentException("Target language must be provided.");
            }

            TargetLanguage = targetLanguage;
            SourceLanguage = sourceLanguage;
            NumberOfSpeakers = numberOfSpeakers;
            Watermark = watermark;
            StartTime = startTime;
            EndTime = endTime;
            HighestResolution = highestResolution;
            DropBackgroundAudio = dropBackgroundAudio;
            UseProfanityFilter = useProfanityFilter;
            ProjectName = projectName;
            var files = new List<DubbingStream>(clips.Count);
            var streams = from audioClip in clips
                          let stream = new MemoryStream(audioClip.EncodeToWav())
                          select (stream, audioClip.name, mediaType: "audio/wav");
            files.AddRange(streams.Select(dub => new DubbingStream(dub.stream, dub.name, dub.mediaType)));
            Files = files;
        }

        public DubbingRequest(
            List<DubbingStream> files,
            string targetLanguage,
            string sourceLanguage = null,
            int? numberOfSpeakers = null,
            bool? watermark = null,
            int? startTime = null,
            int? endTime = null,
            bool? highestResolution = null,
            bool? dropBackgroundAudio = null,
            bool? useProfanityFilter = null,
            string projectName = null)
            : this(targetLanguage, null, files, null, sourceLanguage, numberOfSpeakers, watermark, startTime, endTime, highestResolution, dropBackgroundAudio, useProfanityFilter, projectName)
        {
        }

        private DubbingRequest(
            string targetLanguage,
            Uri sourceUrl = null,
            List<DubbingStream> files = null,
            IEnumerable<string> filePaths = null,
            string sourceLanguage = null,
            int? numberOfSpeakers = null,
            bool? watermark = null,
            int? startTime = null,
            int? endTime = null,
            bool? highestResolution = null,
            bool? dropBackgroundAudio = null,
            bool? useProfanityFilter = null,
            string projectName = null)
        {
            if (string.IsNullOrWhiteSpace(targetLanguage))
            {
                throw new ArgumentException("Target language must be provided.");
            }

            TargetLanguage = targetLanguage;

            if (filePaths == null && sourceUrl == null)
            {
                throw new ArgumentException("Either sourceUrl or filePaths must be provided.");
            }

            files ??= new List<DubbingStream>();

            if (filePaths != null)
            {
                foreach (var filePath in filePaths)
                {
                    if (string.IsNullOrWhiteSpace(filePath))
                    {
                        throw new ArgumentException("File path cannot be empty.");
                    }

                    var fileInfo = new FileInfo(filePath);

                    if (!fileInfo.Exists)
                    {
                        throw new FileNotFoundException($"File not found: {filePath}");
                    }

                    var stream = fileInfo.OpenRead();
                    var extension = fileInfo.Extension.ToLowerInvariant();
                    var mediaType = extension switch
                    {
                        ".3gp" => "video/3gpp",
                        ".acc" => "audio/aac",
                        ".avi" => "video/x-msvideo",
                        ".flac" => "audio/flac",
                        ".ogg" => "audio/ogg",
                        ".mov" => "video/quicktime",
                        ".mp3" => "audio/mp3",
                        ".mp4" => "video/mp4",
                        ".raw" => "audio/raw",
                        ".wav" => "audio/wav",
                        ".webm" => "video/webm",
                        _ => "application/octet-stream"
                    };
                    files.Add(new(stream, fileInfo.Name, mediaType));
                }
            }

            Files = files;
            SourceUrl = sourceUrl;
            SourceLanguage = sourceLanguage;
            NumberOfSpeakers = numberOfSpeakers;
            Watermark = watermark;
            StartTime = startTime;
            EndTime = endTime;
            HighestResolution = highestResolution;
            DropBackgroundAudio = dropBackgroundAudio;
            UseProfanityFilter = useProfanityFilter;
            ProjectName = projectName;
        }

        ~DubbingRequest() => Dispose(false);

        /// <summary>
        /// Files to dub.
        /// </summary>
        public IReadOnlyList<DubbingStream> Files { get; }

        /// <summary>
        /// URL of the source video/audio file.
        /// </summary>
        public Uri SourceUrl { get; }

        /// <summary>
        /// Source language.
        /// </summary>
        /// <remarks>
        /// A list of supported languages can be found at: https://elevenlabs.io/docs/api-reference/how-to-dub-a-video#list-of-supported-languages-for-dubbing
        /// </remarks>
        public string SourceLanguage { get; }

        /// <summary>
        /// The Target language to dub the content into. Can be none if dubbing studio editor is enabled and running manual mode
        /// </summary>
        /// <remarks>
        /// A list of supported languages can be found at: https://elevenlabs.io/docs/api-reference/how-to-dub-a-video#list-of-supported-languages-for-dubbing
        /// </remarks>
        public string TargetLanguage { get; }

        /// <summary>
        /// Number of speakers to use for the dubbing. Set to 0 to automatically detect the number of speakers
        /// </summary>
        public int? NumberOfSpeakers { get; }

        /// <summary>
        /// Whether to apply watermark to the output video.
        /// </summary>
        public bool? Watermark { get; }

        /// <summary>
        /// Start time of the source video/audio file.
        /// </summary>
        public int? StartTime { get; }

        /// <summary>
        /// End time of the source video/audio file.
        /// </summary>
        public int? EndTime { get; }

        /// <summary>
        /// Whether to use the highest resolution available.
        /// </summary>
        public bool? HighestResolution { get; }

        /// <summary>
        /// An advanced setting. Whether to drop background audio from the final dub.
        /// This can improve dub quality where it's known that audio shouldn't have a background track such as for speeches or monologues.
        /// </summary>
        public bool? DropBackgroundAudio { get; }

        /// <summary>
        /// [BETA] Whether transcripts should have profanities censored with the words '[censored]'.
        /// </summary>
        public bool? UseProfanityFilter { get; }

        /// <summary>
        /// Name of the dubbing project.
        /// </summary>
        public string ProjectName { get; }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Files == null) { return; }
                foreach (var dub in Files)
                {
                    try
                    {
                        dub.Dispose();
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
