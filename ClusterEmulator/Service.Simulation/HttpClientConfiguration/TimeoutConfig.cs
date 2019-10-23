using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Timeout;

namespace ClusterEmulator.Service.Simulation.HttpClientConfiguration
{
    /// <summary>
    /// Configurable components of a timeout policy.
    /// </summary>
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
        /// Generates a Polly <see cref="TimeoutPolicy{HttpResponseMessage}"/> from the configuration.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <returns>A <see cref="TimeoutPolicy{HttpResponseMessage}"/> instance.</returns>
        public IAsyncPolicy<HttpResponseMessage> AsTypeModel(ILogger logger)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));

            if (double.IsNaN(TimeoutInSeconds))
            {
                logger.LogCritical("{PolicyConfig} : {Property} is not a valid number", nameof(TimeoutConfig), "time");
                throw new InvalidOperationException("time cannot be NaN");
            }

            // Restrict to valid TimeSpan
            if (TimeoutInSeconds < 0.0d || TimeoutInSeconds > 922337203685.0d)
            {
                logger.LogCritical("{PolicyConfig} : {Property} is outside the valid range", nameof(TimeoutConfig), "time");
                throw new InvalidOperationException("time must be in the range 0 - 922337203685");
            }

            var wait = TimeSpan.FromSeconds(TimeoutInSeconds);
            var strategy = CancelDelegates ? TimeoutStrategy.Optimistic : TimeoutStrategy.Pessimistic;

            return Policy.TimeoutAsync<HttpResponseMessage>(wait, strategy,
                onTimeoutAsync: (context, timespan, task) =>
                {
                    // Log the error and return the task, which should be faulted at this point
                    logger.LogWarning("{PolicyKey} at {OperationKey}: execution timed out after {TimeoutTime} seconds",
                          context.PolicyKey, context.OperationKey, timespan.TotalSeconds);

                    // Handle attempted co-operative cancellation where it is not supported by the delegate
                    return task ?? (CancelDelegates ? Task.CompletedTask : null);
                });
        }
    }
}
