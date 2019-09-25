using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using Policy = ClusterEmulator.Service.Simulation.HttpClientConfiguration.PolicyExtensions;

namespace ClusterEmulator.Service.Simulation.HttpClientConfiguration
{
    /// <summary>
    /// Configurable components of an advanced circuit breaker policy.
    /// </summary>
    public class AdvancedCircuitBreakerConfig : IPolicyConfiguration
    {
        /// <summary>
        /// The break duration in seconds
        /// </summary>
        [JsonProperty("breakDuration")]
        [JsonRequired]
        [Range(0.02d, double.MaxValue, ErrorMessage = "breakDuration must be greater than 20ms")]
        public double BreakDuration { get; set; }


        /// <summary>
        /// The failure threshold as a decimal representation of percentage
        /// </summary>
        [JsonProperty("threshold")]
        [JsonRequired]
        [Range(0.0d, 1.0d, ErrorMessage = "threshold must be between 0 and 1")]
        public double FailureThreshold { get; set; }


        /// <summary>
        /// The sampling duration in seconds
        /// </summary>
        [JsonProperty("samplingDuration")]
        [JsonRequired]
        [Range(0.02d, double.MaxValue, ErrorMessage = "samplingDuration must be greater than 20ms")]
        public double SamplingDuration { get; set; }


        /// <summary>
        /// The minimum circuit throughput required before assessing break conditions
        /// </summary>
        [JsonProperty("throughput")]
        [JsonRequired]
        [Range(0, int.MaxValue, ErrorMessage = "throughput cannot be negative")]
        public int MinimumThroughput { get; set; }


        /// <summary>
        /// Generates a Polly <see cref="CircuitBreakerPolicy{HttpResponseMessage}"/> from the configuration.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <returns>A <see cref="CircuitBreakerPolicy{HttpResponseMessage}"/> instance.</returns>
        public IAsyncPolicy<HttpResponseMessage> AsPolicy(ILogger logger)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));

            if (BreakDuration < 0.02d)
            {
                logger.LogCritical("{PolicyConfig} : {Property} must be greater than 20ms", nameof(AdvancedCircuitBreakerConfig), "breakDuration");
                throw new InvalidOperationException("breakDuration must be greater than 20ms");
            }

            if (FailureThreshold < 0.0d || FailureThreshold > 1.0d)
            {
                logger.LogCritical("{PolicyConfig} : {Property} must be between 0 and 1", nameof(AdvancedCircuitBreakerConfig), "threshold");
                throw new InvalidOperationException("threshold must be between 0 and 1");
            }

            if (SamplingDuration < 0.02d)
            {
                logger.LogCritical("{PolicyConfig} : {Property} must be greater than 20ms", nameof(AdvancedCircuitBreakerConfig), "samplingDuration");
                throw new InvalidOperationException("samplingDuration must be greater than 20ms");
            }

            if (MinimumThroughput < 0)
            {
                logger.LogCritical("{PolicyConfig} : {Property} is negative", nameof(AdvancedCircuitBreakerConfig), "throughput");
                throw new InvalidOperationException("throughput cannot be negative");
            }

            // Create delegates
            void OnBreak(DelegateResult<HttpResponseMessage> result, TimeSpan timespan, Context context)
            {
                logger.LogError(result.Exception, "{PolicyKey} at {OperationKey}: {CircuitState} triggered for {CircuitDelay} seconds due to {StatusCode}",
                    context.PolicyKey, context.OperationKey, CircuitState.Open, timespan.TotalSeconds, result.Result?.StatusCode);
            }

            void OnReset(Context context)
            {
                logger.LogInformation("{PolicyKey} at {OperationKey}: {CircuitState} triggered",
                    context.PolicyKey, context.OperationKey, CircuitState.Closed);
            }

            void OnHalfOpen() => logger.LogDebug("{CircuitState} triggered", CircuitState.HalfOpen);

            // Create policy
            var breaker = Policy
                .HandleHttpRequests()
                .AdvancedCircuitBreakerAsync(
                    failureThreshold: FailureThreshold, 
                    samplingDuration: TimeSpan.FromSeconds(SamplingDuration), 
                    minimumThroughput: MinimumThroughput,
                    durationOfBreak: TimeSpan.FromSeconds(BreakDuration),
                    onBreak: OnBreak,
                    onReset: OnReset,
                    onHalfOpen: OnHalfOpen);

            return breaker;
        }
    }
}
