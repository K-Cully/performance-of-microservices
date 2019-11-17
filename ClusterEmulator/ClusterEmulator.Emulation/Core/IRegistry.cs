using ClusterEmulator.Emulation.Processors;
using ClusterEmulator.Emulation.Steps;
using ClusterEmulator.Emulation.HttpClientConfiguration;
using Polly;
using Polly.Registry;
using System.Collections.Generic;
using System.Net.Http;

namespace ClusterEmulator.Emulation.Core
{
    /// <summary>
    /// Interface for handling retrieval of simulation configuration for a service.
    /// </summary>
    public interface IRegistry
    {
        /// <summary>
        /// Gets the policy registry.
        /// </summary>
        IPolicyRegistry<string> PolicyRegistry { get; }


        /// <summary>
        /// Gets the list of http clients and their configs.
        /// </summary>
        IEnumerable<KeyValuePair<string, ClientConfig>> Clients { get; }


        /// <summary>
        /// Adds the <see cref="IHttpClientFactory"/> instance to any steps which reuse clients.
        /// </summary>
        /// <param name="httpClientFactory">The http client factory</param>
        void ConfigureHttpClients(IHttpClientFactory httpClientFactory);


        /// <summary>
        /// Retrieves the client config with a given name, if it is registered.
        /// </summary>
        /// <param name="name">The name of the client.</param>
        /// <returns>The <see cref="ClientConfig"/> instance.</returns>
        ClientConfig GetClient(string name);


        /// <summary>
        /// Retrieves the policy with a given name, if it is registered.
        /// </summary>
        /// <param name="name">The name of the policy.</param>
        /// <returns>The <see cref="IAsyncPolicy"/> instance.</returns>
        IAsyncPolicy<HttpResponseMessage> GetPolicy(string name);


        /// <summary>
        /// Retrieves the  request processor with a given name, if it is registered.
        /// </summary>
        /// <param name="name">The name of the processor.</param>
        /// <returns>The <see cref="IRequestProcessor"/> instance.</returns>
        IRequestProcessor GetRequestProcessor(string name);


        /// <summary>
        /// Retrieves all registered startup processors.
        /// </summary>
        /// <returns>An enumerable of registered <see cref="IStartupProcessor"/> instances.</returns>
        IEnumerable<IStartupProcessor> GetStartupProcessors();


        /// <summary>
        /// Retrieves the step with a given name, if it is registered.
        /// </summary>
        /// <param name="name">The name of the step.</param>
        /// <returns>The <see cref="IStep"/> instance.</returns>
        IStep GetStep(string name);
    }
}