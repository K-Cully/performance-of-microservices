using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Bulkhead;
//using Policy = CoreService.Simulation.HttpClientConfiguration.PolicyExtensions;

namespace CoreService.Simulation.HttpClientConfiguration
{
    /// <summary>
    /// Configurable components of a circuit breaker policy.
    /// </summary>
    public class BulkheadConfig : IPolicyConfiguration
    {
        /// <summary>
        /// The maximimum number of concurrent requests in the bulkhead
        /// </summary>
        [JsonProperty("bandwidth")]
        [JsonRequired]
        [Range(1, int.MaxValue, ErrorMessage = "bandwidth must be greater than 0")]
        public int MaxParallelization { get; set; }


        /// <summary>
        /// The number of requests to allow to queue
        /// </summary>
        [JsonProperty("queueLength")]
        [Range(1, int.MaxValue, ErrorMessage = "queueLength must be greater than 0")]
        public int? MaxQueuingActions { get; set; }


        /// <summary>
        /// Generates a Polly <see cref="BulkheadPolicy{HttpResponseMessage}"/> from the configuration.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <returns>A <see cref="BulkheadPolicy{HttpResponseMessage}"/> instance.</returns>
        public IAsyncPolicy<HttpResponseMessage> AsPolicy(ILogger logger)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));

            if (MaxParallelization < 1)
            {
                logger.LogCritical("{PolicyConfig} : {Property} must be greater than 0", nameof(BulkheadConfig), "bandwidth");
                throw new InvalidOperationException("bandwidth must be greater than 0");
            }

            if (MaxQueuingActions.HasValue && MaxQueuingActions.Value < 1)
            {
                logger.LogCritical("{PolicyConfig} : {Property} must be greater than 0", nameof(BulkheadConfig), "queueLength");
                throw new InvalidOperationException("queueLength must be greater than 0");
            }

            // Create delegates
            Task OnBulkheadRejectedAsync(Context context)
            {
                logger.LogInformation("{PolicyKey} at {OperationKey}: Request rejected by bulkhead",
                    context.PolicyKey, context.OperationKey);
                return Task.CompletedTask;
            }

            // Generate policy
            var policy = MaxQueuingActions.HasValue ?
                Policy.BulkheadAsync<HttpResponseMessage>(
                    MaxParallelization, MaxQueuingActions.Value, OnBulkheadRejectedAsync)
                : Policy.BulkheadAsync<HttpResponseMessage>(
                    MaxParallelization, OnBulkheadRejectedAsync);

            return policy;
        }
    }
}
