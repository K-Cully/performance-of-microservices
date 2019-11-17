using ClusterEmulator.ServiceFabric.Configuration;
using ClusterEmulator.Emulation.Core;
using ClusterEmulator.Emulation.HttpClientConfiguration;
using ClusterEmulator.Emulation.Processors;
using ClusterEmulator.Emulation.Steps;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using System;
using System.Fabric;
using System.Net.Http;

namespace ClusterEmulator.ServiceFabric.Extensions
{
    /// <summary>
    /// Contains extensions for registering components with a service collection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers components that are required by the simulation engine.
        /// </summary>
        /// <param name="serviceCollection">The DI service collection.</param>
        /// <param name="serviceContext">The Fabric service context.</param>
        /// <returns>The <see cref="IServiceCollection"/> with all simulation engine components registered.</returns>
        public static IServiceCollection AddSimulationEngine(this IServiceCollection serviceCollection, ServiceContext serviceContext)
        {
            // TODO: separate SF specific items from core items

            _ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            _ = serviceContext?.CodePackageActivationContext ?? throw new ArgumentNullException(nameof(serviceContext));

            return serviceCollection
                .AddSingleton(serviceContext.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings)
                .AddSingleton<IRegistrySettings, FabricConfigurationSettings>()
                .AddSingleton<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>, NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>>()
                .AddSingleton<IConfigFactory<IStep>, NestedConfigFactory<IStep, IStep>>()
                .AddSingleton<IConfigFactory<IProcessor>, NestedConfigFactory<IProcessor, IProcessor>>()
                .AddSingleton<IConfigFactory<ClientConfig>, ConfigFactory<ClientConfig>>()
                .AddSingleton<IRegistry, Registry>()
                .AddScoped<IEngine, Engine>();
        }
    }
}
