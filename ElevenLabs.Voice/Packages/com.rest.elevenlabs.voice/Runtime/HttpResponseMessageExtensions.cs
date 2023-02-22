// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ElevenLabs
{
    internal static class HttpResponseMessageExtensions
    {
        public static async Task<string> ReadAsStringAsync(this HttpResponseMessage response, bool debugResponse = false, [CallerMemberName] string methodName = null)
        {
            var responseAsString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"{methodName} Failed!\n{response.RequestMessage}\n[{response.StatusCode}] {responseAsString}");
            }

            if (debugResponse)
            {
                Debug.Log($"{response.RequestMessage}\n[{response.StatusCode}] {responseAsString}");
            }

            return responseAsString;
        }

        internal static async Task CheckResponseAsync(this HttpResponseMessage response, CancellationToken cancellationToken = default, [CallerMemberName] string methodName = null)
        {
            if (!response.IsSuccessStatusCode)
            {
                var responseAsString = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"{methodName} Failed! HTTP status code: {response.StatusCode} | Response body: {responseAsString}");
            }
        }
    }
}
