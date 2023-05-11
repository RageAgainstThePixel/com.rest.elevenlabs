// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System;
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
            var api = new ElevenLabsClient(ElevenLabsAuthentication.LoadFromEnv());
            Assert.NotNull(api.TextToSpeechEndpoint);
            var voice = Voices.Voice.Adam;
            Assert.NotNull(voice);
            var defaultVoiceSettings = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
            var (clipPath, audioClip) = await api.TextToSpeechEndpoint.TextToSpeechAsync("The quick brown fox jumps over the lazy dog.", voice, defaultVoiceSettings, deleteCachedFile: true);
            Assert.NotNull(audioClip);
            Debug.Log(clipPath);
        }

        [Test]
        public async Task Test_02_StreamTextToSpeech()
        {
            var api = new ElevenLabsClient(ElevenLabsAuthentication.LoadFromEnv());
            Assert.NotNull(api.TextToSpeechEndpoint);
            var voice = (await api.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
            Assert.NotNull(voice);
            var defaultVoiceSettings = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
            var tcs = new TaskCompletionSource<AudioClip>();
            var (clipPath, audioClip) = await api.TextToSpeechEndpoint.StreamTextToSpeechAsync(
                "The quick brown fox jumps over the lazy dog.",
                voice,
                clip =>
                {
                    tcs.TrySetResult(clip);
                },
                defaultVoiceSettings,
                deleteCachedFile: true);
            Assert.NotNull(audioClip);
            Debug.Log(clipPath);
            var raisedAudioClip = await tcs.Task;
            Assert.NotNull(raisedAudioClip);
        }
    }
}
