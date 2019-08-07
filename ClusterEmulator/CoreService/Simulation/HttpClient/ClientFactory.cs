using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreService.Simulation.HttpClient
{
    // TODO: refactor/merge with ProcessorFactory

    /// <summary>
    /// Creates processor instances.
    /// </summary>
    public class ClientFactory : IClientFactory
    {
        private List<string> errors;


        /// <summary>
        /// Creates a concrete client config object from a setting value.
        /// </summary>
        /// <param name="settingValue">The client setting value.</param>
        /// <returns>An initialized <see cref="ClientConfig"/> instance.</returns>
        /// <remarks>
        /// Expected setting form:
        /// { <object> }
        /// </remarks>
        public ClientConfig Create(string settingValue)
        {
            if (string.IsNullOrWhiteSpace(settingValue))
            {
                throw new ArgumentException($"{nameof(settingValue)} cannot be null or whitespace");
            }

            ClientConfig client = JsonConvert.DeserializeObject<ClientConfig>(settingValue, SerializerSettings);
            if (errors.Any())
            {
                // TODO: log errors
                return null;
            }

            return client;
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
