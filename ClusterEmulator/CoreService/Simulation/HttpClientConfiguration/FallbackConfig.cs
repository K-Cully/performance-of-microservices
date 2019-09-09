﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Fallback;
using Policy = CoreService.Simulation.HttpClientConfiguration.PolicyExtensions;

namespace CoreService.Simulation.HttpClientConfiguration
{
    /// <summary>
    /// Configurable components of a fallback policy.
    /// </summary>
    public class FallbackConfig : IPolicyConfiguration
    {
        /// <summary>
        /// The response message content as UTF-8 encoded JSON.
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }


        /// <summary>
        /// The response message reason phrase
        /// </summary>
        [JsonProperty("reason")]
        public string Reason { get; set; }


        /// <summary>
        /// The fallback response message status code
        /// </summary>
        [JsonProperty("statusCode")]
        [JsonRequired]
        [Range(0, int.MaxValue, ErrorMessage = "statusCode cannot be negative")]
        public int Status { get; set; }


        /// <summary>
        /// Generates a Polly <see cref="FallbackPolicy"/> from the configuration.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <returns>A <see cref="FallbackPolicy"/> instance.</returns>
        public IAsyncPolicy<HttpResponseMessage> AsPolicy(ILogger logger)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));

            if (Status < 0)
            {
                logger.LogCritical("{PolicyConfig} : {Property} is negative", nameof(FallbackConfig), "statusCode");
                throw new InvalidOperationException("statusCode cannot be negative");
            }

            var message = new HttpResponseMessage
            {
                StatusCode = (HttpStatusCode)Status,
                ReasonPhrase = Reason,
                Content = new StringContent(Content ?? string.Empty, Encoding.UTF8, "application/json")
            };

            logger.LogDebug("Created fallback mesage {StatusCode} - {ReasonPhrase} - {Content}",
                message.StatusCode, message.ReasonPhrase, message.Content);

            return Policy.HandleHttpRequests()
                .FallbackAsync(message, onFallbackAsync: async (result, context) =>
                {
                    logger.LogWarning("{PolicyKey} at {OperationKey}: fallback value substituted, due to: {Exception}.",
                          context.PolicyKey, context.OperationKey, result.Exception);
                    await Task.CompletedTask;
                });
        }
    }
}
