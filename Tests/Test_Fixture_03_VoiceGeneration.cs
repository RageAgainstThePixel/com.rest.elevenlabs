// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.VoiceGeneration;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ElevenLabs.Tests
{
    internal class Test_Fixture_03_VoiceGeneration : AbstractTestFixture
    {
        [Test]
        public async Task Test_01_GetVoiceGenerationOptions()
        {
            try
            {
                Assert.NotNull(ElevenLabsClient.VoiceGenerationEndpoint);
                var options = await ElevenLabsClient.VoiceGenerationEndpoint.GetVoiceGenerationOptionsAsync();
                Assert.NotNull(options);
                Debug.Log(JsonConvert.SerializeObject(options));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        [Test]
        public async Task Test_02_GenerateVoice()
        {
            try
            {
                Assert.NotNull(ElevenLabsClient.VoiceGenerationEndpoint);
                var options = await ElevenLabsClient.VoiceGenerationEndpoint.GetVoiceGenerationOptionsAsync();
                var generateRequest = new GeneratedVoicePreviewRequest("First we thought the PC was a calculator. Then we found out how to turn numbers into letters and we thought it was a typewriter.", options.Genders.FirstOrDefault(), options.Accents.FirstOrDefault(), options.Ages.FirstOrDefault());
                var (generatedVoiceId, audioClip) = await ElevenLabsClient.VoiceGenerationEndpoint.GenerateVoicePreviewAsync(generateRequest);
                Debug.Log(generatedVoiceId);
                Assert.NotNull(audioClip);
                var createVoiceRequest = new CreateVoiceRequest("Test Voice Lab Create Voice", "This is a test voice", generatedVoiceId);
                Assert.NotNull(createVoiceRequest);
                var result = await ElevenLabsClient.VoiceGenerationEndpoint.CreateVoiceAsync(createVoiceRequest);
                Assert.NotNull(result);
                Debug.Log(result.Id);
                var deleteResult = await ElevenLabsClient.VoicesEndpoint.DeleteVoiceAsync(result.Id);
                Assert.NotNull(deleteResult);
                Assert.IsTrue(deleteResult);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
    }
}
