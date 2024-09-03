// Licensed under the MIT License. See LICENSE in the project root for license information.

using ElevenLabs.SoundGeneration;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ElevenLabs.Tests
{
    internal class Test_Fixture_06_SoundGenerationEndpoint : AbstractTestFixture
    {
        [Test]
        public async Task Test_01_GenerateSound()
        {
            Assert.NotNull(ElevenLabsClient.SoundGenerationEndpoint);
            var request = new SoundGenerationRequest("Star Wars Light Saber parry");
            var clip = await ElevenLabsClient.SoundGenerationEndpoint.GenerateSoundAsync(request);
            Assert.NotNull(clip);
            Assert.IsTrue(clip.AudioClip != null);
            Assert.IsTrue(clip.AudioClip.length > 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(clip.Text));
        }
    }
}
