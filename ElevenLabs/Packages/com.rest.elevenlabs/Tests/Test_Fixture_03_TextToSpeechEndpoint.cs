// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ElevenLabs.Voice.Tests
{
    internal class Test_Fixture_03_TextToSpeechEndpoint
    {
        [Test]
        public async Task Test_01_TextToSpeech()
        {
            var api = new ElevenLabsClient(ElevenLabsAuthentication.Default.LoadFromEnvironment());
            Assert.NotNull(api.TextToSpeechEndpoint);
            var voice = Voices.Voice.Adam;
            Assert.NotNull(voice);
            var defaultVoiceSettings = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
            var downloadItem = await api.TextToSpeechEndpoint.TextToSpeechAsync("The quick brown fox jumps over the lazy dog.", voice, defaultVoiceSettings);
            Assert.NotNull(downloadItem.AudioClip);
            Debug.Log(downloadItem.CachedPath);
        }

        [Test]
        public async Task Test_02_StreamTextToSpeech()
        {
            var api = new ElevenLabsClient(ElevenLabsAuthentication.Default.LoadFromEnvironment());
            Assert.NotNull(api.TextToSpeechEndpoint);
            var voice = (await api.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
            Assert.NotNull(voice);
            var defaultVoiceSettings = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
            var partialClips = new Queue<AudioClip>();
            var downloadItem = await api.TextToSpeechEndpoint.StreamTextToSpeechAsync(
                 "The quick brown fox jumps over the lazy dog.",
                 voice,
                 clip => partialClips.Enqueue(clip),
                 defaultVoiceSettings);
            Assert.NotNull(partialClips);
            Assert.IsNotEmpty(partialClips);
            Assert.NotNull(downloadItem);
            Assert.IsNotNull(downloadItem.AudioClip);
        }
    }
}
