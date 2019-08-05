using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Linq;
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
        /// A value indicating if the policy should allow awaiting or not
        /// </summary>
        [JsonProperty("async")]
        [JsonRequired]
        public bool Async;


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
        /// <returns>A <see cref="RetryPolicy"/> instance.</returns>
        public IsPolicy AsPolicy()
        {
            // TODO: UTs and comments

            // TODO: add a logging function call to all policies

            List<double> delays = DelaysInSeconds.ToList();
            bool forever = Retries < 1;
            bool immediate = !delays.Any();
            bool exponential = delays.Count == 1 && delays.First() == -1;

            PolicyBuilder builder = Policy.Handle<HttpRequestException>();

            if (immediate)
            {
                if (Async && forever)
                {
                    return builder.RetryForeverAsync();
                }
                else if (forever)
                {
                    return builder.RetryForever();
                }
                else if (Async)
                {
                    return builder.RetryAsync(Retries);
                }
                else
                {
                    return builder.Retry(Retries);
                }
            }

            if (exponential)
            {
                if (Async && forever)
                {
                    return builder.WaitAndRetryForeverAsync(c => ExponentialDelay(c));
                }
                else if (forever)
                {
                    return builder.WaitAndRetryForever(c => ExponentialDelay(c));
                }
                else if (Async)
                {
                    return builder.WaitAndRetryAsync(Retries, c => ExponentialDelay(c));
                }
                else
                {
                    return builder.WaitAndRetry(Retries, c => ExponentialDelay(c));
                }
            }

            // Use delays only
            if (delays.Any(d => d < 0))
            {
                throw new InvalidOperationException($"delay values cannot be negative");
            }

            if (Retries < delays.Count)
            {
                throw new InvalidOperationException($"retries cannot be lower than the number of delays");
            }

            // Extend delays length to number of retries
            IEnumerable<TimeSpan> delaySpans = delays
                .Concat(Enumerable.Repeat(delays.Last(), delays.Count - Retries))
                .Select(d => DelayWithJitter(d));

            if (Async)
            {
                return builder.WaitAndRetryAsync(delaySpans);
            }
            else
            {
                return builder.WaitAndRetry(delaySpans);
            }
        }


        private TimeSpan ExponentialDelay(int retry)
        {
            return DelayWithJitter(Math.Pow(2, retry));
        }


        private TimeSpan DelayWithJitter(double delay)
        {
            TimeSpan jitter;

            if (JitterMilliseconds < 1)
            {
                jitter = TimeSpan.Zero;
            }
            else
            {
                var jitterer = new Random();
                return TimeSpan.FromMilliseconds(jitterer.Next(0, JitterMilliseconds));
            }

            return TimeSpan.FromSeconds(delay) + jitter;
        }
    }
}
