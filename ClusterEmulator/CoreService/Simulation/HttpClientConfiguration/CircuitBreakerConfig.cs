﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using Policy = CoreService.Simulation.HttpClientConfiguration.PolicyExtensions;

namespace CoreService.Simulation.HttpClientConfiguration
{
    public class CircuitBreakerConfig : IPolicyConfiguration
    {
        /// <summary>
        /// The break duration in seconds
        /// </summary>
        [JsonProperty("duration")]
        [JsonRequired]
        [Range(0.02d, double.MaxValue, ErrorMessage = "duration must be greater than 20ms")]
        public double BreakDuration { get; set; }


        /// <summary>
        /// The number of exceptions to allow before breaking
        /// </summary>
        [JsonProperty("tolerance")]
        [JsonRequired]
        [Range(1, int.MaxValue, ErrorMessage = "tolerance must be greater than 0")]
        public int FaultTolerance { get; set; }


        /// <summary>
        /// Generates a Polly <see cref="CircuitBreakerPolicy"/> from the configuration.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <returns>A <see cref="CircuitBreakerPolicy"/> instance.</returns>
        public IAsyncPolicy<HttpResponseMessage> AsPolicy(ILogger logger)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));

            if (BreakDuration < 0.02d)
            {
                logger.LogCritical("{PolicyConfig} : {Property} must be greater than 20ms", nameof(CircuitBreakerConfig), "duration");
                throw new InvalidOperationException("duration must be greater than 20ms");
            }

            if (FaultTolerance < 1)
            {
                logger.LogCritical("{PolicyConfig} : {Property} must be greater than 0", nameof(CircuitBreakerConfig), "tolerance");
                throw new InvalidOperationException("tolerance must be greater than 0");
            }

            // Create delegates
            void onBreak(DelegateResult<HttpResponseMessage> result, TimeSpan timespan, Context context)
            {
                logger.LogError(result.Exception, "{PolicyKey} at {OperationKey}: {CircuitState} triggered for {CircuitDelay} seconds due to {StatusCode}",
                    context.PolicyKey, context.OperationKey, CircuitState.Open, timespan.TotalSeconds, result.Result?.StatusCode);
            }

            void onReset(Context context)
            {
                logger.LogInformation("{PolicyKey} at {OperationKey}: {CircuitState} triggered",
                    context.PolicyKey, context.OperationKey, CircuitState.Closed);
            }

            void onHalfOpen() => logger.LogDebug("{CircuitState} triggered", CircuitState.HalfOpen);

            // Create policy
            var breaker = Policy
                .HandleHttpRequests()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: FaultTolerance,
                    durationOfBreak: TimeSpan.FromSeconds(BreakDuration),
                    onBreak: onBreak,
                    onReset: onReset,
                    onHalfOpen: onHalfOpen
                );

            // TODO: add handling of BrokenCircuitException and IsolatedCircuitException? in callers
            // BrokenCircuitException contains the actual exception as it's inner exception

            return breaker;
        }
    }
}
