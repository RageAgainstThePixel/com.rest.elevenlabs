// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace ElevenLabs.Extensions
{
    internal static class QueryParamsExtensions
    {
        public static Dictionary<string, string> ToQueryParams(this object @object)
        {
            var parameters = new Dictionary<string, string>();
            var json = JsonConvert.SerializeObject(@object, ElevenLabsClient.JsonSerializationOptions);
            // parse the json into a dictionary
            var jObject = JObject.Parse(json);

            foreach (var property in jObject.Properties())
            {
                switch (property.Value.Type)
                {
                    case JTokenType.Array:
                    {
                        // Flatten arrays as comma-separated values
                        var array = string.Join(",", ((JArray)property.Value).Select(v => v.ToString()));

                        if (!string.IsNullOrWhiteSpace(array))
                        {
                            parameters.Add(property.Name, array);
                        }

                        break;
                    }
                    default:
                        if (property.Value.Type != JTokenType.Null &&
                            property.Value.Type != JTokenType.None)
                        {
                            var value = property.Value.ToString();

                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                parameters.Add(property.Name, value);
                            }
                        }

                        break;
                }
            }

            return parameters;
        }
    }
}
