using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreService.Simulation.Processors
{
    /// <summary>
    /// Creates processor instances.
    /// </summary>
    public class ProcessorFactory : IProcessorFactory
    {
        private List<string> errors;


        /// <summary>
        /// Creates a concrete step object from a setting value.
        /// </summary>
        /// <param name="settingValue">The processor setting value.</param>
        /// <returns>An initialized <see cref="IProcessor"/> instance.</returns>
        /// <remarks>
        /// Expected setting form:
        /// { <object>
        /// </remarks>
        public IProcessor Create(string settingValue)
        {
            if (string.IsNullOrWhiteSpace(settingValue))
            {
                throw new ArgumentException($"{nameof(settingValue)} cannot be null or whitespace");
            }

            IProcessor processor = JsonConvert.DeserializeObject<Processor>(settingValue, SerializerSettings);
            if (errors.Any())
            {
                // TODO: log errors
                return null;
            }

            return processor;
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
