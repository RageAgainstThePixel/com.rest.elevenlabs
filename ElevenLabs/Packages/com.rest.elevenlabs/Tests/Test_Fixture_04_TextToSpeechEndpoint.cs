// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ElevenLabs.Tests
{
    internal class Test_Fixture_04_TextToSpeechEndpoint : AbstractTestFixture
    {
        [Test]
        public async Task Test_01_TextToSpeech()
        {
            Assert.NotNull(ElevenLabsClient.TextToSpeechEndpoint);
            var voice = Voices.Voice.Adam;
            Assert.NotNull(voice);
            var defaultVoiceSettings = await ElevenLabsClient.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
            var voiceClip = await ElevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync("The quick brown fox jumps over the lazy dog.", voice, defaultVoiceSettings);
            Assert.NotNull(voiceClip);
            Assert.NotNull(voiceClip.AudioClip);
            Debug.Log(voiceClip.Id);
        }

        [Test]
        public async Task Test_02_StreamTextToSpeech()
        {
            Assert.NotNull(ElevenLabsClient.TextToSpeechEndpoint);
            var voice = (await ElevenLabsClient.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
            Assert.NotNull(voice);
            var defaultVoiceSettings = await ElevenLabsClient.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
            var partialClips = new Queue<AudioClip>();
            var voiceClip = await ElevenLabsClient.TextToSpeechEndpoint.StreamTextToSpeechAsync("The quick brown fox jumps over the lazy dog.", voice,
                 clip => partialClips.Enqueue(clip),
                 defaultVoiceSettings);
            Assert.NotNull(partialClips);
            Assert.IsNotEmpty(partialClips);
            Assert.NotNull(voiceClip);
            Assert.IsNotNull(voiceClip.AudioClip);
            Debug.Log(voiceClip.Id);
        }
    }
}
