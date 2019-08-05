using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreService.Simulation.Steps
{
    /// <summary>
    /// Creates step instances.
    /// </summary>
    public class StepFactory : IStepFactory
    {
        private readonly string stepNamespace = typeof(StepFactory).Namespace;


        private List<string> errors;


        /// <summary>
        /// Creates a concrete step object from a setting value.
        /// </summary>
        /// <param name="settingValue">The step setting value.</param>
        /// <returns>An initialized <see cref="IStep"/> instance.</returns>
        /// <remarks>
        /// Expected setting form:
        /// { type : <typename>, step : { <object> } }
        /// </remarks>
        public IStep Create(string settingValue)
        {
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
            Type type = Type.GetType($"{stepNamespace}.{json.type.Value}");
            if (type is null)
            {
                throw new InvalidOperationException($"{stepNamespace}.{json.type.Value} did not resolve to a Type");
            }

            // Convert the step JSON object to the identified concrete type
            JObject stepJson = json.step;
            if (stepJson is null)
            {
                throw new InvalidOperationException($"No step found in setting '{settingValue}'");
            }

            var serializer = JsonSerializer.CreateDefault(SerializerSettings);
            IStep step = stepJson.ToObject(type, serializer) as IStep;
            if (errors.Any())
            {
                // TODO: log errors
                return null;
            }

            return step;
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
