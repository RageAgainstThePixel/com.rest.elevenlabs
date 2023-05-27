// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using ElevenLabs.Models;
using ElevenLabs.Voices;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace ElevenLabs.TextToSpeech
{
    [Preserve]
    internal sealed class TextToSpeechRequest
    {
        [JsonConstructor]
        public TextToSpeechRequest(
            [JsonProperty("text")] string text,
            [JsonProperty("model_id")] Model model,
            [JsonProperty("voice_settings")] VoiceSettings voiceSettings)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(nameof(text));
            }

            Text = text;

            Model = model ?? throw new ArgumentNullException(nameof(model));
            VoiceSettings = voiceSettings ?? throw new ArgumentNullException(nameof(voiceSettings));
        }

        [Preserve]
        [JsonProperty("text")]
        public string Text { get; }

        [Preserve]
        [JsonProperty("model_id")]
        public string Model { get; }

        [Preserve]
        [JsonProperty("voice_settings")]
        public VoiceSettings VoiceSettings { get; }
    }
}
