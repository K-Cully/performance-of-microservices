using ClusterEmulator.Service.Simulation.Core;
using Polly;
using System.Net.Http;

namespace ClusterEmulator.Service.Simulation.HttpClientConfiguration
{
    /// <summary>
    /// Convienence wrapper for configurable components of a Polly policy.
    /// </summary>
    public interface IPolicyConfiguration : IConfigModel<IAsyncPolicy<HttpResponseMessage>>
    { }
}