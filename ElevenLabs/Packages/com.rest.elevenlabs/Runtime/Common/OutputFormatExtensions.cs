// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace ElevenLabs
{
    public static class OutputFormatExtensions
    {
        public static AudioType GetAudioType(this OutputFormat format)
            => format switch
            {
                OutputFormat.MP3_44100_64 => AudioType.MPEG,
                OutputFormat.MP3_44100_96 => AudioType.MPEG,
                OutputFormat.MP3_44100_128 => AudioType.MPEG,
                OutputFormat.MP3_44100_192 => AudioType.MPEG,
                OutputFormat.PCM_16000 => AudioType.OGGVORBIS,
                OutputFormat.PCM_22050 => AudioType.OGGVORBIS,
                OutputFormat.PCM_24000 => AudioType.OGGVORBIS,
                OutputFormat.PCM_44100 => AudioType.OGGVORBIS,
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
            };
    }
}
