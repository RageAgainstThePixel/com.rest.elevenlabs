// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ElevenLabs.Voices
{
    [Preserve]
    public sealed class Sample
    {
        [Preserve]
        public static implicit operator string(Sample sample) => sample.Id;

        [Preserve]
        [JsonConstructor]
        public Sample(
            [JsonProperty("sample_id")] string id,
            [JsonProperty("file_name")] string fileName,
            [JsonProperty("mime_type")] string mimeType,
            [JsonProperty("size_bytes")] int sizeBytes,
            [JsonProperty("hash")] string hash)
        {
            Id = id;
            FileName = fileName;
            MimeType = mimeType;
            SizeBytes = sizeBytes;
            Hash = hash;
        }

        [Preserve]
        [JsonProperty("sample_id")]
        public string Id { get; }

        [Preserve]
        [JsonProperty("file_name")]
        public string FileName { get; }

        [Preserve]
        [JsonProperty("mime_type")]
        public string MimeType { get; }

        [Preserve]
        [JsonProperty("size_bytes")]
        public int SizeBytes { get; }

        [Preserve]
        [JsonProperty("hash")]
        public string Hash { get; }
    }
}
