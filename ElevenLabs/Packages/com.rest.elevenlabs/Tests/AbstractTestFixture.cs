// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace ElevenLabs.Tests
{
    internal abstract class AbstractTestFixture
    {
        protected readonly ElevenLabsClient ElevenLabsClient;

        protected AbstractTestFixture()
        {
            var auth = new ElevenLabsAuthentication().LoadDefaultsReversed();
            var settings = new ElevenLabsSettings();
            ElevenLabsClient = new ElevenLabsClient(auth, settings);
            ElevenLabsClient.EnableDebug = true;
        }
    }
}
