// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net.Http;
using System.Runtime.CompilerServices;
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
    }
}
