// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.Serialization;

namespace ElevenLabs.Dubbing
{
    public enum DubbingFormat
    {
        [EnumMember(Value = "srt")]
        Srt,
        [EnumMember(Value = "webvtt")]
        WebVtt
    }
}
