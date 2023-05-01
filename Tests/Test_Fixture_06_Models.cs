// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace ElevenLabs.Voice.Tests
{
    internal class Test_Fixture_06_Models
    {
        public async Task Test_01_GetModels()
        {
            var api = new ElevenLabsClient(ElevenLabsAuthentication.LoadFromEnv());
            Assert.NotNull(api.ModelsEndpoint);
            var models = await api.ModelsEndpoint.GetModelsAsync();
            Assert.NotNull(models);
            Assert.IsNotEmpty(models);

            foreach (var model in models)
            {
                Debug.Log($"{model.Id} | {model.Name} | {model.Description}");

                foreach (var language in model.Languages)
                {
                    Debug.Log($"    {language.Id} | {language.Name}");
                }
            }
        }
    }
}
