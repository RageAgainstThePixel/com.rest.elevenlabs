// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.Serialization;

namespace ElevenLabs.Voices
{
    /// <summary>
    /// State of the voice’s fine-tuning to filter by. Applicable only to professional voices clones.
    /// </summary>
    public enum FineTuningStateTypes
    {
        /// <summary>
        /// Draft state.
        /// </summary>
        [EnumMember(Value = "draft")]
        Draft,
        /// <summary>
        /// Not verified state.
        /// </summary>
        [EnumMember(Value = "not_verified")]
        NotVerified,
        /// <summary>
        /// Not started state.
        /// </summary>
        [EnumMember(Value = "not_started")]
        NotStarted,
        /// <summary>
        /// Queued state.
        /// </summary>
        [EnumMember(Value = "queued")]
        Queued,
        /// <summary>
        /// Fine-tuning in progress.
        /// </summary>
        [EnumMember(Value = "fine_tuning")]
        FineTuning,
        /// <summary>
        /// Fine-tuned state.
        /// </summary>
        [EnumMember(Value = "fine_tuned")]
        FineTuned,
        /// <summary>
        /// Failed state.
        /// </summary>
        [EnumMember(Value = "failed")]
        Failed,
        /// <summary>
        /// Delayed state.
        /// </summary>
        [EnumMember(Value = "delayed")]
        Delayed
    }
}
