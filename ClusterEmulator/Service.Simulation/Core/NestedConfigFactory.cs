using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace ClusterEmulator.Service.Simulation.Core
{
    /// <summary>
    /// Creates concrete instances of the specifed type.
    /// </summary>
    /// <typeparam name="TConfigModel">The generic type stored in the setting values.</typeparam>
    /// <typeparam name="TModel">The generic type to generate from setting values.</typeparam>
    public class NestedConfigFactory<TConfigModel, TModel> : ConfigFactory<TModel>, IConfigFactory<TModel>
        where TConfigModel : class
        where TModel : class
    {
        private readonly Func<TConfigModel, ILogger, TModel> conversion;
        private readonly ILoggerFactory logFactory;
        private readonly Type ModelType = typeof(TModel);
        private readonly Type ConfigType = typeof(TConfigModel);
        private readonly string configNamespace = typeof(TConfigModel).Namespace;


        /// <summary>
        /// Initializes a new instance of <see cref="NestedConfigFactory{TConfigModel, TModel}"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> instance to use for initializing loggers for created objects.</param>
        /// <param name="converter">An optional converter between <see cref="TConfigModel"/> and <see cref="TModel"/> instances to finalize creation. If null an as</param>
        public NestedConfigFactory(
            ILogger<NestedConfigFactory<TConfigModel, TModel>> logger,
            ILoggerFactory loggerFactory,
            Func<TConfigModel, ILogger, TModel> converter = null)
            : base(logger)
        {
            logFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            conversion = converter;

            if (conversion is null && !ModelType.IsAssignableFrom(ConfigType))
            {
                throw new ArgumentException("Model is not assignable from config. A converter must be provided.", nameof(converter));
            }
        }


        /// <summary>
        /// Creates a concrete object from a setting value.
        /// </summary>
        /// <param name="settingValue">The setting value.</param>
        /// <returns>An initialized <see cref="TModel"/> instance.</returns>
        /// <remarks>
        /// Expected setting form:
        /// { type : <typename>, value : { <object> } }
        /// </remarks>
        public override TModel Create(string settingValue)
        {
            if (string.IsNullOrWhiteSpace(settingValue))
            {
                throw new ArgumentException($"{nameof(settingValue)} cannot be null or whitespace");
            }

            log.LogInformation("Creating {ConfigType} from {SettingValue}", typeof(TModel).Name, settingValue);

            // Deserialize JSON to dynamic object
            dynamic json = JsonConvert.DeserializeObject(settingValue, SerializerSettings);
            if (Errors.Any())
            {
                foreach (string error in Errors)
                {
                    log.LogError("'{JsonError}' encountered deserializing {SettingValue}",
                        error, settingValue);
                }

                return null;
            }

            if (json?.type?.Value is null)
            {
                log.LogError("'{SettingError}' encountered deserializing {SettingValue}",
                    "Type not found", settingValue);
                return null;
            }

            // Extract the model type
            string typeName = $"{configNamespace}.{json.type.Value}";
            Type type = Type.GetType(typeName);
            if (type is null)
            {
                log.LogError("'{SettingError}' encountered deserializing {SettingValue}",
                    $"{typeName} is not recognised", settingValue);
                throw new InvalidOperationException($"{typeName} did not resolve to a Type");
            }

            // Convert the value JSON object to the identified concrete type
            JObject cvalueJson = json.value;
            if (cvalueJson is null)
            {
                log.LogError("'{SettingError}' encountered deserializing {SettingValue}",
                    "No value found", settingValue);
                throw new InvalidOperationException($"No value found in setting '{settingValue}'");
            }

            var serializer = JsonSerializer.CreateDefault(SerializerSettings);
            TConfigModel config = cvalueJson.ToObject(type, serializer) as TConfigModel;
            if (Errors.Any())
            {
                foreach (string error in Errors)
                {
                    log.LogError("'{JsonError}' encountered deserializing {SettingValue}",
                        error, settingValue);
                }

                return null;
            }

            if (conversion is null)
            {
                return config as TModel;
            }

            ILogger typeLogger = logFactory.CreateLogger(type);
            return conversion(config, typeLogger);
        }
    }
}
