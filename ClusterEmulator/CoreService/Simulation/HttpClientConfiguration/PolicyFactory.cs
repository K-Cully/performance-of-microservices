using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreService.Simulation.HttpClientConfiguration
{
    /// <summary>
    /// Creates policy instances.
    /// </summary>
    public class PolicyFactory : IPolicyFactory
    {
        private readonly string policyNamespace = typeof(PolicyFactory).Namespace;


        private List<string> errors;


        /// <summary>
        /// Creates a concrete policy object from a setting value.
        /// </summary>
        /// <param name="settingValue">The step setting value.</param>
        /// <returns>An initialized <see cref="Policy"/> instance.</returns>
        /// <remarks>
        /// Expected setting form:
        /// { type : <typename>, policy : { <object> } }
        /// </remarks>
        public Policy Create(string settingValue)
        {
            // TODO: flesh out properly
            // See docs: https://github.com/App-vNext/Polly/blob/master/README.md#retry

            if (string.IsNullOrWhiteSpace(settingValue))
            {
                throw new ArgumentException($"{nameof(settingValue)} cannot be null or whitespace");
            }

            // Deserialize JSON to dynamic object
            dynamic json = JsonConvert.DeserializeObject(settingValue, SerializerSettings);
            if (errors.Any())
            {
                // TODO: log errors
                return null;
            }

            if (json?.type?.Value is null)
            {
                // TODO: log error
                return null;
            }

            // Extract the step type
            Type type = Type.GetType($"{policyNamespace}.{json.type.Value}");
            if (type is null)
            {
                throw new InvalidOperationException($"{policyNamespace}.{json.type.Value} did not resolve to a Type");
            }

            // Convert the step JSON object to the identified concrete type
            JObject policyJson = json.policy;
            if (policyJson is null)
            {
                throw new InvalidOperationException($"No policy found in setting '{settingValue}'");
            }

            var serializer = JsonSerializer.CreateDefault(SerializerSettings);
            var config = policyJson.ToObject(type, serializer) as IPolicyConfiguration;
            if (errors.Any())
            {
                // TODO: log errors
                return null;
            }

            return config.AsPolicy();
        }


        private JsonSerializerSettings SerializerSettings
        {
            get
            {
                errors = new List<string>();
                if (serializerSettings is null)
                {
                    serializerSettings = new JsonSerializerSettings()
                    {
                        Error = (o, e) =>
                        {
                            e.ErrorContext.Handled = true;
                            errors.Add(e.ErrorContext?.Error?.Message);
                        },
                        NullValueHandling = NullValueHandling.Ignore,
                    };
                }

                return serializerSettings;
            }
        }


        private JsonSerializerSettings serializerSettings;
    }
}
