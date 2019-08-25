using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Timeout;

namespace CoreService.Simulation.HttpClientConfiguration
{
    /// <summary>
    /// Configurable components of a timeout policy.
    /// </summary>
    [Serializable]
    public class TimeoutConfig : IPolicyConfiguration
    {
        /// <summary>
        /// A value indicating if the policy should timeout with cancellation of delegates or hard timeout.
        /// </summary>
        [JsonProperty("cancelDelegates")]
        [JsonRequired]
        public bool CancelDelegates;


        /// <summary>
        /// The length of the timeout in seconds.
        /// </summary>
        [JsonProperty("time")]
        [JsonRequired]
        [Range(0.0d, 922337203685.0d, ErrorMessage = "time must be in the range 0 - 922337203685")]
        public double TimeoutInSeconds;


        /// <summary>
        /// Generates a Polly <see cref="TimeoutPolicy"/> from the configuration.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <returns>A <see cref="TimeoutPolicy"/> instance.</returns>
        public IAsyncPolicy AsPolicy(ILogger logger)
        {
            // Restrict to valid TimeSpan
            if (TimeoutInSeconds < 0.0d || TimeoutInSeconds > 922337203685.0d)
            {
                throw new InvalidOperationException("time must be in the range 0 - 922337203685");
            }

            if (double.IsNaN(TimeoutInSeconds))
            {
                throw new InvalidOperationException("time cannot be NaN");
            }

            // TODO: add a logging function call to all policies

            var wait = TimeSpan.FromSeconds(TimeoutInSeconds);
            var strategy = CancelDelegates ? TimeoutStrategy.Optimistic : TimeoutStrategy.Pessimistic;

            return Policy.TimeoutAsync(wait, strategy);
        }
    }
}
