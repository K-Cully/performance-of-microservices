using ClusterEmulator.ServiceFabric.Configuration;
using ClusterEmulator.Emulation.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Fabric;

namespace ClusterEmulator.ServiceFabric.Extensions
{
    /// <summary>
    /// Contains extensions for registering components with a service collection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers settings from the service context with as simulation settings.
        /// </summary>
        /// <param name="serviceCollection">The DI service collection.</param>
        /// <param name="serviceContext">The Fabric service context.</param>
        /// <returns>The <see cref="IServiceCollection"/> with simulation engine settings registered.</returns>
        public static IServiceCollection AddSimulationSettings(this IServiceCollection serviceCollection, ServiceContext serviceContext)
        {
            _ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            _ = serviceContext?.CodePackageActivationContext ?? throw new ArgumentNullException(nameof(serviceContext));

            return serviceCollection
                .AddSingleton(serviceContext.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings)
                .AddSingleton<IRegistrySettings, FabricConfigurationSettings>();
        }
    }
}
