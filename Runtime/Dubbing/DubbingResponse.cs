// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ElevenLabs.Dubbing
{
    [Preserve]
    public sealed class DubbingResponse
    {
        [Preserve]
        [JsonConstructor]
        internal DubbingResponse(
            [JsonProperty("dubbing_id")] string dubbingId,
            [JsonProperty("expected_duration_sec")] float expectedDuration)
        {
            DubbingId = dubbingId;
            ExpectedDuration = expectedDuration;
        }

        [Preserve]
        [JsonProperty("dubbing_id")]
        public string DubbingId { get; }

        [Preserve]
        [JsonProperty("expected_duration_sec")]
        public float ExpectedDuration { get; }

        [Preserve]
        public static implicit operator string(DubbingResponse response) => response?.DubbingId;
    }
}
