using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreService.Simulation.Core
{
    /// <summary>
    /// Creates concrete instances of the specifed type.
    /// </summary>
    /// <typeparam name="TModel">The type to generate from setting values.</typeparam>
    public class ConfigFactory<TModel> : IConfigFactory<TModel>
        where TModel : class
    {
        private readonly ILogger<ConfigFactory<TModel>> log;
        private List<string> errors;


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
        public TModel Create(string settingValue)
        {
            if (string.IsNullOrWhiteSpace(settingValue))
            {
                throw new ArgumentException($"{nameof(settingValue)} cannot be null or whitespace");
            }

            log.LogInformation("Creating {ConfigType} from {SettingValue}", typeof(TModel).Name, settingValue);

            TModel value = JsonConvert.DeserializeObject<TModel>(settingValue, SerializerSettings);
            if (errors.Any())
            {
                foreach(string error in errors)
                {
                    log.LogError("Deserializing {SettingValue} encountered {JsonError}", settingValue, error);
                }

                return null;
            }

            return value;
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
