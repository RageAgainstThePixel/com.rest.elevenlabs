// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.Serialization;

namespace ElevenLabs
{
    public enum TextNormalization
    {
        [EnumMember(Value = "auto")]
        Auto,
        [EnumMember(Value = "on")]
        On,
        [EnumMember(Value = "off")]
        Off
    }
}
