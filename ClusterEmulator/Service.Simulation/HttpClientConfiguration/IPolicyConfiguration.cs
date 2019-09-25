using Microsoft.Extensions.Logging;
using Polly;
using System.Net.Http;

namespace ClusterEmulator.Service.Simulation.HttpClientConfiguration
{
    /// <summary>
    /// Configurable components of a Polly policy.
    /// </summary>
    public interface IPolicyConfiguration
    {
        /// <summary>
        /// Generates a Polly <see cref="IAsyncPolicy"/> from the configuration.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <returns>A <see cref="IAsyncPolicy"/> instance.</returns>
        IAsyncPolicy<HttpResponseMessage> AsPolicy(ILogger logger);
    }
}