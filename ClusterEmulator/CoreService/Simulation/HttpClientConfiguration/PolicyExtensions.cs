using Polly;
using Polly.Timeout;
using System.Net.Http;

namespace CoreService.Simulation.HttpClientConfiguration
{
    /// <summary>
    /// Extensions for the <see cref="Policy"/> class.
    /// </summary>
    public static class PolicyExtensions
    {
        /// <summary>
        /// Sets up the policy to handle http requests with the default settings for this project.
        /// </summary>
        /// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
        public static PolicyBuilder<HttpResponseMessage> HandleHttpRequests()
        {
            return Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutRejectedException>()
                .OrResult<HttpResponseMessage>(message => !message.IsSuccessStatusCode);
        }
    }
}
