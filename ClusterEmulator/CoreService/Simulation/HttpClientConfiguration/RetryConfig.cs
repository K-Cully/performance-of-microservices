﻿using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace CoreService.Simulation.HttpClientConfiguration
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
        public IAsyncPolicy AsPolicy()
        {
            if (DelaysInSeconds is null)
            {
                throw new InvalidOperationException("delays cannot be null");
            }

            if (JitterMilliseconds < 0)
            {
                throw new InvalidOperationException("jitter cannot be negative");
            }

            // TODO: add a logging function call to all policies

            // TODO: test condition and/or chage to HandleResult

            var builder = Policy
                .Handle<HttpRequestException>();
            List<double> delays = DelaysInSeconds.ToList();
            bool forever = Retries < 1;

            bool immediate = !delays.Any();
            if (immediate)
            {
                if (forever)
                {
                    return builder.RetryForeverAsync();
                }
                else
                {
                    return builder.RetryAsync(Retries);
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
                return builder.WaitAndRetryForeverAsync(c => Delay(c, exponential));
            }
            else
            {
                return builder.WaitAndRetryAsync(Retries, c => Delay(c, exponential));
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