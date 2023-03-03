// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs;
using NUnit.Framework;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rest.ElevenLabs.Voice.Tests
{
    internal class Test_Fixture_03_TextToSpeechEndpoint
    {
        [UnityTest]
        public IEnumerator Test_01_TextToSpeech()
        {
            yield return AwaitTestUtilities.Await(async () =>
            {
                var api = new ElevenLabsClient(ElevenLabsAuthentication.LoadFromEnv());
                Assert.NotNull(api.TextToSpeechEndpoint);
                var voice = (await api.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
                Assert.NotNull(voice);
                var defaultVoiceSettings = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
                var (clipPath, audioClip) = await api.TextToSpeechEndpoint.TextToSpeechAsync("The quick brown fox jumps over the lazy dog.", voice, defaultVoiceSettings);
                Assert.NotNull(audioClip);
                Debug.Log(clipPath);
            });
        }

        //[UnityTest]
        //public IEnumerator Test_02_TextToSpeechStream()
        //{
        //    yield return AwaitTestUtilities.Await(async () =>
        //    {
        //        TODO
        //        await Task.CompletedTask;
        //    });
        //}
    }
}
