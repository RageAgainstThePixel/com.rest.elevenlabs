// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace ElevenLabs.Dubbing
{
    [Preserve]
    public sealed class DubbingProjectMetadata
    {
        [Preserve]
        [JsonConstructor]
        internal DubbingProjectMetadata(
            [JsonProperty("dubbing_id")] string dubbingId,
            [JsonProperty("name")] string name,
            [JsonProperty("status")] string status,
            [JsonProperty("target_languages")] IReadOnlyList<string> targetLanguages,
            [JsonProperty("error")] string error)
        {
            DubbingId = dubbingId;
            Name = name;
            Status = status;
            TargetLanguages = targetLanguages;
            Error = error;
        }

        [Preserve]
        [JsonProperty("dubbing_id")]
        public string DubbingId { get; }

        [Preserve]
        [JsonProperty("name")]
        public string Name { get; }

        [Preserve]
        [JsonProperty("status")]
        public string Status { get; }

        [Preserve]
        [JsonProperty("target_languages")]
        public IReadOnlyList<string> TargetLanguages { get; }

        [Preserve]
        [JsonProperty("error")]
        public string Error { get; }

        [JsonIgnore]
        public float ExpectedDurationSeconds { get; internal set; }

        [JsonIgnore]
        public TimeSpan TimeCompleted { get; internal set; }
    }
}
