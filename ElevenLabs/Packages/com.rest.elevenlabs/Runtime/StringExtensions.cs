// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace ElevenLabs
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Generates a <see cref="Guid"/> based on the string.
        /// </summary>
        /// <param name="string">The string to generate the <see cref="Guid"/>.</param>
        /// <returns>A new <see cref="Guid"/> that represents the string.</returns>
        public static Guid GenerateGuid(this string @string)
        {
            using MD5 md5 = MD5.Create();
            return new Guid(md5.ComputeHash(Encoding.Default.GetBytes(@string)));
        }

        /// <summary>
        /// Encodes the json string content.
        /// </summary>
        public static StringContent ToJsonStringContent(this string json)
            => new StringContent(json, Encoding.UTF8, "application/json");

        /// <summary>
        /// Create a new directory based on the current string format.
        /// </summary>
        /// <param name="parentDirectory"></param>
        /// <param name="newDirectoryName"></param>
        /// <returns>Full path to the newly created directory.</returns>
        public static string CreateNewDirectory(this string parentDirectory, string newDirectoryName)
        {
            if (string.IsNullOrWhiteSpace(parentDirectory))
            {
                throw new ArgumentNullException(nameof(parentDirectory));
            }

            if (string.IsNullOrWhiteSpace(newDirectoryName))
            {
                throw new ArgumentNullException(nameof(newDirectoryName));
            }

            var voiceDirectory = Path.Combine(parentDirectory, newDirectoryName);

            if (!Directory.Exists(voiceDirectory))
            {
                Directory.CreateDirectory(voiceDirectory);
            }

            return voiceDirectory;
        }
    }
}
