// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace ElevenLabs
{
    public enum OutputFormat
    {
        /// <summary>
        /// mp3 with 44.1kHz sample rate at 64kbps.
        /// </summary>
        MP3_44100_64,
        /// <summary>
        /// mp3 with 44.1kHz sample rate at 96kbps.
        /// </summary>
        MP3_44100_96,
        /// <summary>
        /// mp3 with 44.1kHz sample rate at 128kbps.
        /// </summary>
        MP3_44100_128,
        /// <summary>
        /// mp3 with 44.1kHz sample rate at 192kbps.<br/>
        /// </summary>
        /// <remarks>
        /// Requires you to be subscribed to Creator tier or above.
        /// </remarks>
        MP3_44100_192,
        /// <summary>
        /// PCM format (S16LE) with 16kHz sample rate.
        /// </summary>
        PCM_16000,
        /// <summary>
        /// PCM format (S16LE) with 22.05kHz sample rate.
        /// </summary>
        PCM_22050,
        /// <summary>
        /// PCM format (S16LE) with 24kHz sample rate.
        /// </summary>
        PCM_24000,
        /// <summary>
        /// PCM format (S16LE) with 44.1kHz sample rate.
        /// </summary>
        /// <remarks>
        /// Requires you to be subscribed to Independent Publisher tier or above.
        /// </remarks>
        PCM_44100
    }
}
