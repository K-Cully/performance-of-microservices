using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.Net.Http;

// TODO: UTs

namespace CoreService.Simulation.HttpClient
{
    /// <summary>
    /// Creates policy instances.
    /// </summary>
    public class PolicyFactory : IPolicyFactory
    {
        private List<string> errors;


        /// <summary>
        /// Creates a concrete policy object from a setting value.
        /// </summary>
        /// <param name="settingValue">The step setting value.</param>
        /// <returns>An initialized <see cref="IsPolicy"/> instance.</returns>
        /// <remarks>
        /// Expected setting form:
        /// 
        /// TODO: fill this in
        /// 
        /// 
        /// { type : <typename>, policy : { <object> } }
        /// </remarks>
        public IsPolicy Create(string settingValue)
        {
            // TODO: flesh out properly
            // See docs: https://github.com/App-vNext/Polly/blob/master/README.md#retry

            var timeoutPolicy = Policy.Timeout(5);
            // TODO: add logging to timeout

            var policy = Policy.Handle<HttpRequestException>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3)
                });

            return policy;
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
