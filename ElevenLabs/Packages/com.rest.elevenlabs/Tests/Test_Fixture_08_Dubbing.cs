// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.Dubbing;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Utilities.WebRequestRest;

namespace ElevenLabs.Tests
{
    internal class Test_Fixture_08_Dubbing : AbstractTestFixture
    {
        [Test]
        public async Task Test_01_Dubbing_File()
        {
            try
            {
                Assert.NotNull(ElevenLabsClient.DubbingEndpoint);
                var audioPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath("96e9fdf73bc7a944f93886694973b90e"));
                var request = new DubbingRequest(audioPath, "es", "en", 1, watermark: false);
                var metadata = await ElevenLabsClient.DubbingEndpoint.DubAsync(request, progress: new Progress<DubbingProjectMetadata>(metadata =>
                {
                    switch (metadata.Status)
                    {
                        case "dubbing":
                            Debug.Log($"Dubbing for {metadata.DubbingId} in progress... Expected Duration: {metadata.ExpectedDurationSeconds:0.00} seconds");
                            break;
                        case "dubbed":
                            Debug.Log($"Dubbing for {metadata.DubbingId} complete in {metadata.TimeCompleted.TotalSeconds:0.00} seconds!");
                            break;
                        default:
                            Debug.Log($"Status: {metadata.Status}");
                            break;
                    }
                }));
                Assert.IsFalse(string.IsNullOrEmpty(metadata.DubbingId));
                Assert.IsTrue(metadata.ExpectedDurationSeconds > 0);

                var dubbedClipPath = await ElevenLabsClient.DubbingEndpoint.GetDubbedFileAsync(metadata.DubbingId, request.TargetLanguage);
                Assert.NotNull(dubbedClipPath);
                Assert.IsTrue(File.Exists(dubbedClipPath));
                var dubbedClip = await Rest.DownloadAudioClipAsync($"file://{dubbedClipPath}", AudioType.MPEG);
                Assert.IsNotNull(dubbedClip);
                Assert.IsTrue(dubbedClip.length > 0);

                var srcFile = new FileInfo(audioPath);
                var transcriptPath = new FileInfo($"{srcFile.FullName}.dubbed.{request.TargetLanguage}.srt");
                var transcriptFile = await ElevenLabsClient.DubbingEndpoint.GetTranscriptForDubAsync(metadata.DubbingId, request.TargetLanguage);
                await File.WriteAllTextAsync(transcriptPath.FullName, transcriptFile);
                Assert.IsTrue(transcriptPath.Exists);
                Assert.IsTrue(transcriptPath.Length > 0);

                await ElevenLabsClient.DubbingEndpoint.DeleteDubbingProjectAsync(metadata.DubbingId);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        [Test]
        public async Task Test_02_Dubbing_Url()
        {
            try
            {
                Assert.NotNull(ElevenLabsClient.DubbingEndpoint);

                var request = new DubbingRequest(new Uri("https://youtu.be/Zo5-rhYOlNk?si=xetqANRnve7P6UmX"), "ja", "en", 1, watermark: true, dropBackgroundAudio: true);
                var metadata = await ElevenLabsClient.DubbingEndpoint.DubAsync(request, progress: new Progress<DubbingProjectMetadata>(metadata =>
                {
                    switch (metadata.Status)
                    {
                        case "dubbing":
                            Debug.Log($"Dubbing for {metadata.DubbingId} in progress... Expected Duration: {metadata.ExpectedDurationSeconds:0.00} seconds");
                            break;
                        case "dubbed":
                            Debug.Log($"Dubbing for {metadata.DubbingId} complete in {metadata.TimeCompleted.TotalSeconds:0.00} seconds!");
                            break;
                        default:
                            Debug.Log($"Status: {metadata.Status}");
                            break;
                    }
                }));
                Assert.IsFalse(string.IsNullOrEmpty(metadata.DubbingId));
                Assert.IsTrue(metadata.ExpectedDurationSeconds > 0);

                var assetsDir = Path.GetFullPath(Application.dataPath);
                var dubbedClip = await ElevenLabsClient.DubbingEndpoint.GetDubbedFileAsync(metadata.DubbingId, request.TargetLanguage);
                Assert.IsNotNull(dubbedClip);
                Assert.IsTrue(File.Exists(dubbedClip));

                var transcriptPath = new FileInfo(Path.Combine(assetsDir, $"online.dubbed.{request.TargetLanguage}.srt"));
                var transcriptFile = await ElevenLabsClient.DubbingEndpoint.GetTranscriptForDubAsync(metadata.DubbingId, request.TargetLanguage);
                await File.WriteAllTextAsync(transcriptPath.FullName, transcriptFile);
                Assert.IsTrue(transcriptPath.Exists);
                Assert.IsTrue(transcriptPath.Length > 0);

                await ElevenLabsClient.DubbingEndpoint.DeleteDubbingProjectAsync(metadata.DubbingId);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        [Test]
        public async Task Test_03_Dubbing_AudioClip()
        {
            try
            {
                Assert.NotNull(ElevenLabsClient.DubbingEndpoint);
                var clipPath = AssetDatabase.GUIDToAssetPath("96e9fdf73bc7a944f93886694973b90e");
                var audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
                var request = new DubbingRequest(audioClip, "es", "en", 1, watermark: false);
                var metadata = await ElevenLabsClient.DubbingEndpoint.DubAsync(request, progress: new Progress<DubbingProjectMetadata>(metadata =>
                {
                    switch (metadata.Status)
                    {
                        case "dubbing":
                            Debug.Log($"Dubbing for {metadata.DubbingId} in progress... Expected Duration: {metadata.ExpectedDurationSeconds:0.00} seconds");
                            break;
                        case "dubbed":
                            Debug.Log($"Dubbing for {metadata.DubbingId} complete in {metadata.TimeCompleted.TotalSeconds:0.00} seconds!");
                            break;
                        default:
                            Debug.Log($"Status: {metadata.Status}");
                            break;
                    }
                }));
                Assert.IsFalse(string.IsNullOrEmpty(metadata.DubbingId));
                Assert.IsTrue(metadata.ExpectedDurationSeconds > 0);

                var srcFile = new FileInfo(Path.GetFullPath(clipPath));
                var dubbedClipPath = await ElevenLabsClient.DubbingEndpoint.GetDubbedFileAsync(metadata.DubbingId, request.TargetLanguage);
                Assert.IsNotNull(dubbedClipPath);
                Assert.IsTrue(File.Exists(dubbedClipPath));
                var dubbedClip = await Rest.DownloadAudioClipAsync($"file://{dubbedClipPath}", AudioType.MPEG);
                Assert.IsNotNull(dubbedClip);
                Assert.IsTrue(dubbedClip.length > 0);

                var transcriptPath = new FileInfo($"{srcFile.FullName}.dubbed.{request.TargetLanguage}.srt");
                var transcriptFile = await ElevenLabsClient.DubbingEndpoint.GetTranscriptForDubAsync(metadata.DubbingId, request.TargetLanguage);
                await File.WriteAllTextAsync(transcriptPath.FullName, transcriptFile);
                Assert.IsTrue(transcriptPath.Exists);
                Assert.IsTrue(transcriptPath.Length > 0);

                await ElevenLabsClient.DubbingEndpoint.DeleteDubbingProjectAsync(metadata.DubbingId);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
    }
}
