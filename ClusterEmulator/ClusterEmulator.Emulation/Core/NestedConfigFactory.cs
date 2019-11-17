using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace ClusterEmulator.Emulation.Core
{
    /// <summary>
    /// Creates concrete instances of the specifed type.
    /// </summary>
    /// <typeparam name="TConfigModel">The generic type stored in the setting values.</typeparam>
    /// <typeparam name="TModel">The generic type to generate from setting values.</typeparam>
    public class NestedConfigFactory<TConfigModel, TModel> : ConfigFactory<TModel>, IConfigFactory<TModel>
        where TConfigModel : class, IConfigModel<TModel>
        where TModel : class
    {
        private readonly ILoggerFactory logFactory;
        private readonly string configNamespace = typeof(TConfigModel).Namespace;


        /// <summary>
        /// Initializes a new instance of <see cref="NestedConfigFactory{TConfigModel, TModel}"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> instance to use for initializing loggers for created objects.</param>
        public NestedConfigFactory(
            ILogger<NestedConfigFactory<TConfigModel, TModel>> logger,
            ILoggerFactory loggerFactory)
            : base(logger)
        {
            logFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
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
            JObject valueJson = json.value;
            if (valueJson is null)
            {
                log.LogError("'{SettingError}' encountered deserializing {SettingValue}",
                    "No value found", settingValue);
                throw new InvalidOperationException($"No value found in setting '{settingValue}'");
            }

            var serializer = JsonSerializer.CreateDefault(SerializerSettings);
            TConfigModel config = valueJson.ToObject(type, serializer) as TConfigModel;
            if (Errors.Any())
            {
                foreach (string error in Errors)
                {
                    log.LogError("'{JsonError}' encountered deserializing {SettingValue}",
                        error, settingValue);
                }

                return null;
            }

            ILogger typeLogger = logFactory.CreateLogger(type);
            return config.AsTypeModel(typeLogger);
        }
    }
}
