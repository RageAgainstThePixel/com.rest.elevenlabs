// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.Serialization;

namespace ElevenLabs.Voices
{
    public enum SortDirections
    {
        /// <summary>
        /// Sort by the creation time (Unix timestamp). May not be available for older voices.
        /// </summary>
        [EnumMember(Value = "created_at_unix")]
        CreatedAtUnix,
        /// <summary>
        /// Sort by the name of the voice.
        /// </summary>
        [EnumMember(Value = "name")]
        Name
    }
}
