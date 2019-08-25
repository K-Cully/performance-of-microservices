using Microsoft.Extensions.Logging;
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
        private readonly ILogger<StepFactory> log;
        private readonly ILoggerFactory logFactory;

        private readonly string stepNamespace = typeof(StepFactory).Namespace;
        private List<string> errors;


        /// <summary>
        /// Initializes a new instance of <see cref="StepFactory"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> instance to use for initializing loggers for created objects.</param>
        public StepFactory(ILogger<StepFactory> logger, ILoggerFactory loggerFactory)
        {
            log = logger ?? throw new ArgumentNullException(nameof(logger));
            logFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }


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

            log.LogInformation("Creating {ConfigType} from {SettingValue}", nameof(IStep), settingValue);

            // Deserialize JSON to dynamic object
            dynamic json = JsonConvert.DeserializeObject(settingValue, SerializerSettings);
            if (errors.Any())
            {
                foreach (string error in errors)
                {
                    log.LogError("Deserializing {SettingValue} encountered {JsonError}",
                        settingValue, error);
                }

                return null;
            }

            if (json?.type?.Value is null)
            {
                log.LogError("Deserializing {SettingValue} encountered {SettingError}",
                    settingValue, "Type not found");
                return null;
            }

            // Extract the step type
            string typeName = $"{stepNamespace}.{json.type.Value}";
            Type type = Type.GetType(typeName);
            if (type is null)
            {
                log.LogError("Deserializing {SettingValue} encountered {SettingError}",
                    settingValue, $"{typeName} is not recognised");
                throw new InvalidOperationException($"{typeName} did not resolve to a Type");
            }

            // Convert the step JSON object to the identified concrete type
            JObject stepJson = json.step;
            if (stepJson is null)
            {
                log.LogError("Deserializing {SettingValue} encountered {SettingError}",
                    settingValue, "No step found");
                throw new InvalidOperationException($"No step found in setting '{settingValue}'");
            }

            var serializer = JsonSerializer.CreateDefault(SerializerSettings);

            IStep step = stepJson.ToObject(type, serializer) as IStep;
            if (errors.Any())
            {
                foreach (string error in errors)
                {
                    log.LogError("Deserializing {SettingValue} encountered {JsonError}",
                        settingValue, error);
                }

                return null;
            }

            step.InitializeLogger(logFactory.CreateLogger(type));
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
