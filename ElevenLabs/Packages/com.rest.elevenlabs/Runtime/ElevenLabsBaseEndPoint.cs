// Licensed under the MIT License. See LICENSE in the project root for license information.

using Utilities.WebRequestRest;

namespace ElevenLabs
{
    public abstract class ElevenLabsBaseEndPoint : BaseEndPoint<ElevenLabsClient, ElevenLabsAuthentication, ElevenLabsSettings>
    {
        protected ElevenLabsBaseEndPoint(ElevenLabsClient client) : base(client) { }
    }
}
