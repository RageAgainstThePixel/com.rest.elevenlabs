// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace ElevenLabs.Voices
{
    public sealed class Sample
    {
        public static implicit operator string(Sample sample) => sample.Id;

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

        [JsonProperty("sample_id")]
        public string Id { get; }

        [JsonProperty("file_name")]
        public string FileName { get; }

        [JsonProperty("mime_type")]
        public string MimeType { get; }

        [JsonProperty("size_bytes")]
        public int SizeBytes { get; }

        [JsonProperty("hash")]
        public string Hash { get; }
    }
}
