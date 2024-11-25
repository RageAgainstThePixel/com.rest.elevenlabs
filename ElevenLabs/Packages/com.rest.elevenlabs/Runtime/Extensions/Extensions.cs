// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace ElevenLabs.Extensions
{
    public static class Extensions
    {
        public static int GetSampleRate(this OutputFormat format) => format switch
        {
            OutputFormat.PCM_16000 => 16000,
            OutputFormat.PCM_22050 => 22050,
            OutputFormat.PCM_24000 => 24000,
            OutputFormat.PCM_44100 => 44100,
            _ => 44100
        };
    }
}
