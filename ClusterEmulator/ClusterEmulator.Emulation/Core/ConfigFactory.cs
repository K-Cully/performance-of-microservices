using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClusterEmulator.Emulation.Core
{
    /// <summary>
    /// Creates concrete instances of the specifed type.
    /// </summary>
    /// <typeparam name="TModel">The type to generate from setting values.</typeparam>
    public class ConfigFactory<TModel> : IConfigFactory<TModel>
        where TModel : class
    {
        protected readonly ILogger<ConfigFactory<TModel>> log;
        private JsonSerializerSettings serializerSettings;


        /// <summary>
        /// Initializes a new instance of <see cref="ConfigFactory{TModel}"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        public ConfigFactory(ILogger<ConfigFactory<TModel>> logger)
        {
            log = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        /// Creates a concrete object from a setting value.
        /// </summary>
        /// <param name="settingValue">The setting value.</param>
        /// <returns>An initialized <see cref="TModel"/> instance.</returns>
        /// <remarks>
        /// Expected setting form:
        /// { <object> }
        /// </remarks>
        public virtual TModel Create(string settingValue)
        {
            if (string.IsNullOrWhiteSpace(settingValue))
            {
                throw new ArgumentException($"{nameof(settingValue)} cannot be null or whitespace");
            }

            log.LogInformation("Creating {ConfigType} from {SettingValue}", typeof(TModel).Name, settingValue);
            return DeserializeSettingWithErrorHandling(settingValue);
        }


        /// <summary>
        /// Deserializes a setting value with JSON serialization error handling
        /// </summary>
        /// <param name="settingValue">The JSON setting value</param>
        /// <returns>An instance of <see cref="TModel"/>, if successful. Null otherwise.</returns>
        protected TModel DeserializeSettingWithErrorHandling(string settingValue)
        {
            TModel value = JsonConvert.DeserializeObject<TModel>(settingValue, SerializerSettings);
            if (Errors.Any())
            {
                foreach (string error in Errors)
                {
                    log.LogCritical("'{JsonError}' encountered deserializing {SettingValue}", error, settingValue);
                }

                return null;
            }

            return value;
        }


        protected JsonSerializerSettings SerializerSettings
        {
            get
            {
                Errors = new List<string>();
                if (serializerSettings is null)
                {
                    serializerSettings = new JsonSerializerSettings()
                    {
                        Error = (o, e) =>
                        {
                            e.ErrorContext.Handled = true;
                            Errors.Add(e.ErrorContext?.Error?.Message);
                        },
                        NullValueHandling = NullValueHandling.Ignore,
                    };
                }

                return serializerSettings;
            }
        }

        protected List<string> Errors { get; set; }
    }
}
