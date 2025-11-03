// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Utilities.WebRequestRest;

namespace ElevenLabs.SoundGeneration
{
    public class SoundGenerationEndpoint : ElevenLabsBaseEndPoint
    {
        public SoundGenerationEndpoint(ElevenLabsClient client) : base(client) { }

        protected override string Root => "sound-generation";

        /// <summary>
        /// converts text into sounds & uses the most advanced AI audio model ever.
        /// Create sound effects for your videos, voice-overs or video games.
        /// </summary>
        /// <param name="request"><see cref="SoundGenerationRequest"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="GeneratedClip"/>.</returns>
        public async Task<GeneratedClip> GenerateSoundAsync(SoundGenerationRequest request, CancellationToken cancellationToken = default)
        {
            var payload = JsonConvert.SerializeObject(request, ElevenLabsClient.JsonSerializationOptions);
            var clipId = Guid.NewGuid().ToString();
            var audioClip = await Rest.DownloadAudioClipAsync(
                url: GetUrl(),
                audioType: AudioType.MPEG,
                httpMethod: UnityWebRequest.kHttpVerbPOST,
                fileName: clipId,
                jsonData: payload,
                parameters: new RestParameters(client.DefaultRequestHeaders, debug: EnableDebug),
                cancellationToken: cancellationToken);
            Rest.TryGetDownloadCacheItem(clipId, out var cachedPath);
            return new GeneratedClip(clipId, request.Text, audioClip, cachedPath);
        }
    }
}
