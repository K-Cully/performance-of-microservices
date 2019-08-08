using Polly;

namespace CoreService.Simulation.HttpClientConfiguration
{
    /// <summary>
    /// Configurable components of a Polly policy.
    /// </summary>
    public interface IPolicyConfiguration
    {
        /// <summary>
        /// Generates a Polly <see cref="Policy"/> from the configuration.
        /// </summary>
        /// <returns>A <see cref="Policy"/> instance.</returns>
        Policy AsPolicy();
    }
}