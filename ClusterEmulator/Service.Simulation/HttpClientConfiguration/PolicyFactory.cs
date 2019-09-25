using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace CoreService.Simulation.HttpClientConfiguration
{
    /// <summary>
    /// Creates policy instances.
    /// </summary>
    public class PolicyFactory : IPolicyFactory
    {
        private readonly ILogger<PolicyFactory> log;
        private readonly ILoggerFactory logFactory;
        private readonly string policyNamespace = typeof(PolicyFactory).Namespace;
        private List<string> errors;


        /// <summary>
        /// Initializes a new instance of <see cref="PolicyFactory"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> instance to use for initializing loggers for created objects.</param>
        public PolicyFactory(ILogger<PolicyFactory> logger, ILoggerFactory loggerFactory)
        {
            log = logger ?? throw new ArgumentNullException(nameof(logger));
            logFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }


        /// <summary>
        /// Creates a concrete policy object from a setting value.
        /// </summary>
        /// <param name="settingValue">The step setting value.</param>
        /// <returns>An initialized <see cref="IAsyncPolicy"/> instance.</returns>
        /// <remarks>
        /// Expected setting form:
        /// { type : <typename>, policy : { <object> } }
        /// </remarks>
        public IAsyncPolicy<HttpResponseMessage> Create(string settingValue)
        {
            if (string.IsNullOrWhiteSpace(settingValue))
            {
                throw new ArgumentException($"{nameof(settingValue)} cannot be null or whitespace");
            }

            log.LogInformation("Creating {ConfigType} from {SettingValue}", nameof(IAsyncPolicy), settingValue);

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
            string typeName = $"{policyNamespace}.{json.type.Value}";
            Type type = Type.GetType(typeName);
            if (type is null)
            {
                log.LogError("Deserializing {SettingValue} encountered {SettingError}",
                    settingValue, $"{typeName} is not recognised");
                throw new InvalidOperationException($"{typeName} did not resolve to a Type");
            }

            // Convert the step JSON object to the identified concrete type
            JObject policyJson = json.policy;
            if (policyJson is null)
            {
                log.LogError("Deserializing {SettingValue} encountered {SettingError}",
                    settingValue, "No policy found");
                throw new InvalidOperationException($"No policy found in setting '{settingValue}'");
            }

            var serializer = JsonSerializer.CreateDefault(SerializerSettings);
            var config = policyJson.ToObject(type, serializer) as IPolicyConfiguration;
            if (errors.Any())
            {
                foreach (string error in errors)
                {
                    log.LogError("Deserializing {SettingValue} encountered {JsonError}",
                        settingValue, error);
                }

                return null;
            }

            return config.AsPolicy(logFactory.CreateLogger(type));
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
