using Polly;

namespace CoreService.Simulation.HttpClient
{
    /// <summary>
    /// Configurable components of a Polly policy.
    /// </summary>
    public interface IPolicyConfiguration
    {
        /// <summary>
        /// Generates a Polly <see cref="IsPolicy"/> from the configuration.
        /// </summary>
        /// <returns>A <see cref="IsPolicy"/> instance.</returns>
        IsPolicy AsPolicy();
    }
}