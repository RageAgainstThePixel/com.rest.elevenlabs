// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.Serialization;

namespace ElevenLabs.Voices
{
    /// <summary>
    /// Category of the voice to filter by.
    /// </summary>
    public enum CategoryTypes
    {
        /// <summary>
        /// Premade voice.
        /// </summary>
        [EnumMember(Value = "premade")]
        Premade,
        /// <summary>
        /// Cloned voice.
        /// </summary>
        [EnumMember(Value = "cloned")]
        Cloned,
        /// <summary>
        /// Generated voice.
        /// </summary>
        [EnumMember(Value = "generated")]
        Generated,
        /// <summary>
        /// Professional voice.
        /// </summary>
        [EnumMember(Value = "professional")]
        Professional
    }
}
