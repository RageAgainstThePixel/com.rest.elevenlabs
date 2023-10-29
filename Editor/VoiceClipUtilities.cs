// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using ElevenLabs.Extensions;
using UnityEditor;
using UnityEngine;

namespace ElevenLabs.Editor
{
    public static class VoiceClipUtilities
    {
        /// <summary>
        /// Copies the <see cref="voiceClip"/> into the specified <see cref="directory"/>.
        /// </summary>
        /// <param name="voiceClip">The <see cref="VoiceClip"/> to copy into the project.</param>
        /// <param name="directory">The root directory to copy the <see cref="voiceClip"/> into.</param>
        /// <remarks>
        /// Clips are copied into the root directory, but additional directories will be created for
        /// their respective voice name, and samples.
        /// </remarks>
        public static void CopyIntoProject(this VoiceClip voiceClip, string directory)
            => CopyIntoProject(directory, voiceClip);

        /// <summary>
        /// Copies the <see cref="voiceClips"/> into the specified <see cref="directory"/>.
        /// </summary>
        /// <param name="directory">The root directory to copy the <see cref="voiceClips"/> into.</param>
        /// <param name="voiceClips">The array of <see cref="VoiceClip"/>s to copy into the project.</param>
        /// <remarks>
        /// Clips are copied into the root directory, but additional directories will be created for
        /// their respective voice name, and samples.
        /// </remarks>
        public static void CopyIntoProject(string directory, params VoiceClip[] voiceClips)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                Debug.LogError($"Directory not found! \"{directory}\"");
                return;
            }

            if (voiceClips is not { Length: not 0 }) { return; }

            AssetDatabase.DisallowAutoRefresh();
            var count = 0;

            foreach (var voiceClip in voiceClips)
            {
                EditorUtility.DisplayProgressBar("Importing assets...", $"Importing {voiceClip.Id}", ++count / (float)voiceClips.Length);

                try
                {
                    // TODO replace or strip voice name in case it is too long or has invalid characters
                    var targetDirectory = directory.CreateNewDirectory(voiceClip.Voice.Name);

                    // if the text is null or empty then the voice clip is for a sample
                    if (string.IsNullOrWhiteSpace(voiceClip.Text))
                    {
                        targetDirectory = targetDirectory.CreateNewDirectory("Samples");
                    }

                    var extension = Path.GetExtension(voiceClip.CachedPath);
                    var targetPath = Path.Combine(targetDirectory, $"{voiceClip.Id}{extension}");

                    if (!File.Exists(targetPath))
                    {
                        File.Copy(voiceClip.CachedPath!, targetPath);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            AssetDatabase.AllowAutoRefresh();
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
    }
}
