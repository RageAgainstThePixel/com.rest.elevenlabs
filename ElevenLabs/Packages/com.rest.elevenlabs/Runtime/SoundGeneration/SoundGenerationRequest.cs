// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using UnityEngine.Scripting;

namespace ElevenLabs.SoundGeneration
{
    [Preserve]
    public sealed class SoundGenerationRequest
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text">
        /// The text that will get converted into a sound effect.
        /// </param>
        /// <param name="duration">
        /// The duration of the sound which will be generated in seconds.
        /// Must be at least 0.5 and at most 22.
        /// If set to None we will guess the optimal duration using the prompt.
        /// Defaults to None.
        /// </param>
        /// <param name="promptInfluence">
        /// A higher prompt influence makes your generation follow the prompt more closely while also making generations less variable.
        /// Must be a value between 0 and 1.
        /// Defaults to 0.3.
        /// </param>
        [Preserve]
        public SoundGenerationRequest(string text, float? duration = null, float? promptInfluence = null)
        {
            Text = text;

            if (duration is > 22f or < 0.5f)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be a value between 0.5 and 22.");
            }

            Duration = duration;

            if (promptInfluence is > 1f or < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(promptInfluence), "Prompt influence must be a value between 0 and 1.");
            }

            PromptInfluence = promptInfluence;
        }

        /// <summary>
        /// The text that will get converted into a sound effect.
        /// </summary>
        [Preserve]
        [JsonProperty("text")]
        public string Text { get; }

        /// <summary>
        /// The duration of the sound which will be generated in seconds.
        /// Must be at least 0.5 and at most 22.
        /// If set to None we will guess the optimal duration using the prompt.
        /// Defaults to None.
        /// </summary>
        [Preserve]
        [JsonProperty("duration_seconds")]
        public float? Duration { get; }

        /// <summary>
        /// A higher prompt influence makes your generation follow the prompt more closely while also making generations less variable.
        /// Must be a value between 0 and 1.
        /// Defaults to 0.3.
        /// </summary>
        [Preserve]
        [JsonProperty("prompt_influence")]
        public float? PromptInfluence { get; }
    }
}
