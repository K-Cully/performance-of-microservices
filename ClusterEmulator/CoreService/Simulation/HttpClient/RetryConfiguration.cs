using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace CoreService.Simulation.HttpClient
{
    /// <summary>
    /// Configurable components of a retry policy.
    /// </summary>
    [Serializable]
    public class RetryConfig : IPolicyConfiguration
    {
        /// <summary>
        /// The number of retries to attempt.
        /// Values less than 1 indicate retry forever.
        /// </summary>
        [JsonProperty("retries")]
        [JsonRequired]
        public int Retries;


        /// <summary>
        /// An explicit list of delays to apply before each retry.
        /// </summary>
        /// <remarks>
        /// If shorter than the number of retries, the last delay value
        /// will be used for all excess retries.
        /// A single value of -1 will be treated as exponential backoff.
        /// </remarks>
        [JsonProperty("delays")]
        [JsonRequired]
        public IEnumerable<int> DelaysInSeconds;


        /// <summary>
        /// Artificial jitter to apply to retry delays.
        /// </summary>
        [JsonProperty("jitter")]
        [Range(1, int.MaxValue, ErrorMessage = "jitter cannot be negative")]
        public int JitterMilliseconds;


        /// <summary>
        /// Generates a Polly <see cref="RetryPolicy"/> from the configuration.
        /// </summary>
        /// <returns>A <see cref="RetryPolicy"/> instance.</returns>
        public IsPolicy AsPolicy()
        {
            // TODO: implement fully and comment

            var policy = Policy.Handle<HttpRequestException>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3)
                });

            return policy;
        }
    }
}
