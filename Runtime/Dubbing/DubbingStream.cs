// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;

namespace ElevenLabs.Dubbing
{
    public sealed class DubbingStream : IDisposable
    {
        public DubbingStream(Stream stream, string name, string mediaType)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));

            if (Stream.Length == 0)
            {
                throw new ArgumentException("Stream cannot be empty.");
            }

            if (!Stream.CanRead)
            {
                throw new ArgumentException("Stream must be readable.");
            }

            Name = name ?? throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new ArgumentException("Name cannot be empty.");
            }

            MediaType = mediaType ?? throw new ArgumentNullException(nameof(mediaType));

            if (string.IsNullOrWhiteSpace(MediaType))
            {
                throw new ArgumentException("Media type cannot be empty.");
            }

            if (MediaType.Contains("/"))
            {
                var parts = MediaType.Split('/');

                if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                {
                    throw new ArgumentException("Invalid media type.");
                }
            }
            else
            {
                throw new ArgumentException("Invalid media type.");
            }
        }

        public Stream Stream { get; }

        public string Name { get; }

        public string MediaType { get; }

        public void Dispose()
        {
            Stream?.Dispose();
        }
    }
}
