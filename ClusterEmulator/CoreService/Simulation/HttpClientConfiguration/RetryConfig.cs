using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Polly.Timeout;

namespace CoreService.Simulation.HttpClientConfiguration
{
    /// <summary>
    /// Configurable components of a retry policy.
    /// </summary>
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
        public IEnumerable<double> DelaysInSeconds;


        /// <summary>
        /// Artificial jitter to apply to retry delays.
        /// </summary>
        [JsonProperty("jitter")]
        [Range(1, int.MaxValue, ErrorMessage = "jitter cannot be negative")]
        public int JitterMilliseconds;


        /// <summary>
        /// Generates a Polly <see cref="RetryPolicy"/> from the configuration.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <returns>A <see cref="RetryPolicy"/> instance.</returns>
        public IAsyncPolicy<HttpResponseMessage> AsPolicy(ILogger logger)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));

            if (DelaysInSeconds is null)
            {
                logger.LogCritical("{PolicyConfig} : {Property} is not initialized", nameof(RetryConfig), "delays");
                throw new InvalidOperationException("delays cannot be null");
            }

            if (JitterMilliseconds < 0)
            {
                logger.LogCritical("{PolicyConfig} : {Property} is negative", nameof(RetryConfig), "jitter");
                throw new InvalidOperationException("jitter cannot be negative");
            }

            var builder = Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutRejectedException>()
                .OrResult<HttpResponseMessage>(message => !message.IsSuccessStatusCode);

            List<double> delays = DelaysInSeconds.ToList();
            bool forever = Retries < 1;

            bool immediate = !delays.Any();
            if (immediate)
            {
                if (forever)
                {
                    return builder.RetryForeverAsync(onRetry: result =>
                       logger.LogError("{RetryPolicy} failed with {StatusCode}", "RetryForeverAsync", result?.Result?.StatusCode));
                }
                else
                {
                    return builder.RetryAsync(Retries, onRetry: (result, count) =>
                        logger.LogError("{RetryPolicy} failed with {StatusCode} on attempt {RetryCount}", "RetryAsync",
                            result?.Result?.StatusCode, count));
                }
            }

            bool exponential = delays.Count == 1 && delays.First() == -1.0d;

            // Defined delays rely on the list being initialized with positive delay values 
            if (!exponential && delays.Any(d => d < 0))
            {
                throw new InvalidOperationException($"delay values cannot be negative");
            }

            if (forever)
            {
                return builder.WaitAndRetryForeverAsync(
                    sleepDurationProvider: count => Delay(count, exponential),
                    onRetry: (result, timespan) => logger.LogError(
                                    "{RetryPolicy} failed with {StatusCode} after {RetryTime} seconds",
                                    "WaitAndRetryForeverAsync", result?.Result?.StatusCode, timespan.TotalSeconds));
            }
            else
            {
                return builder.WaitAndRetryAsync(
                    Retries,
                    sleepDurationProvider: count => Delay(count, exponential),
                    onRetry: (result, timespan, count, context) => logger.LogError(
                                            "{RetryPolicy} failed with {StatusCode} on attempt {RetryCount} after {RetryTime} seconds",
                                            "WaitAndRetryAsync", result?.Result?.StatusCode, count, timespan.TotalSeconds));
            }
        }


        private TimeSpan Delay(int retry, bool exponential)
        {
            if (exponential)
            {
                return ExponentialDelay(retry);
            }

            List<double> delays = DelaysInSeconds.ToList();
            if (retry > delays.Count)
            {
                return DelayWithJitter(delays.Last());
            }

            // retry - 1 to convert from count to index
            return DelayWithJitter(delays[retry - 1]);
        }


        private TimeSpan ExponentialDelay(int retry)
        {
            return DelayWithJitter(Math.Pow(2, retry));
        }


        private TimeSpan DelayWithJitter(double delay)
        {
            var jitterer = new Random();
            TimeSpan jitter = TimeSpan.FromMilliseconds(jitterer.Next(0, JitterMilliseconds));
            return TimeSpan.FromSeconds(delay) + jitter;
        }
    }
}
