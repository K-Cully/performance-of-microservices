using ClusterEmulator.Emulation.Core;
using ClusterEmulator.Emulation.HttpClientConfiguration;
using ClusterEmulator.Emulation.Processors;
using ClusterEmulator.Emulation.Steps;
using Microsoft.Extensions.DependencyInjection;
//using Polly;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ClusterEmulator.Emulation.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers components that are required by the simulation engine.
        /// </summary>
        /// <param name="serviceCollection">The DI service collection.</param>
        /// <returns>The <see cref="IServiceCollection"/> with all simulation engine components registered.</returns>
        public static IServiceCollection AddSimulationEngine(this IServiceCollection serviceCollection)
        {
            _ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));

            return serviceCollection
                .AddSingleton<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>, NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>>()
                .AddSingleton<IConfigFactory<IStep>, NestedConfigFactory<IStep, IStep>>()
                .AddSingleton<IConfigFactory<IProcessor>, NestedConfigFactory<IProcessor, IProcessor>>()
                .AddSingleton<IConfigFactory<ClientConfig>, ConfigFactory<ClientConfig>>()
                .AddSingleton<IRegistry, Registry>()
                .AddScoped<IEngine, Engine>();
        }


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
