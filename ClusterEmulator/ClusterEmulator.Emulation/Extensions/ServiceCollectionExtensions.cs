using ClusterEmulator.Emulation.Core;
using ClusterEmulator.Emulation.HttpClientConfiguration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace ClusterEmulator.Emulation.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers configured clients and policies with the DI container and HTTP client factory.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="registry">The simulation registry containing all configuration state.</param>
        public static void AddSimulationEngineClients(this IServiceCollection services, IRegistry registry)
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            _ = registry ?? throw new ArgumentNullException(nameof(registry));

            if (registry.PolicyRegistry is null || registry.Clients is null)
            {
                throw new InvalidOperationException(
                    $"{nameof(registry.PolicyRegistry)} and {nameof(registry.Clients)} must be initialized.");
            }

            // Register initialised policy registry
            services.AddPolicyRegistry(registry.PolicyRegistry);

            // Enumerate all registered clients
            var clients = new List<KeyValuePair<string, ClientConfig>>(registry.Clients);
            foreach ((string name, var config) in clients)
            {
                IHttpClientBuilder builder = services.AddHttpClient(name, c =>
                {
                    c.BaseAddress = new Uri(config.BaseAddress);
                    if (config.RequestHeaders != null)
                    {
                        foreach ((string key, string value) in config.RequestHeaders)
                        {
                            c.DefaultRequestHeaders.Add(key, value);
                        }
                    }
                });

                // Add Polly policies to http client builder.
                // Note that any policies added through the http client factory initialization
                // must be of type IAsyncPolicy<HttpResponseMessage> or Polly will throw errors.
                foreach (string policy in config.Policies)
                {
                    builder = builder.AddPolicyHandlerFromRegistry(policy);
                }
            }
        }
    }
}
