﻿using ClusterEmulator.Service.Shared.Configuration;
using ClusterEmulator.Service.Simulation.Core;
using ClusterEmulator.Service.Simulation.HttpClientConfiguration;
using ClusterEmulator.Service.Simulation.Processors;
using ClusterEmulator.Service.Simulation.Steps;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Fabric;

namespace ClusterEmulator.Service.Shared.Extensions
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
            _ = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            _ = serviceContext?.CodePackageActivationContext ?? throw new ArgumentNullException(nameof(serviceContext));

            return serviceCollection
                .AddSingleton(serviceContext.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings)
                .AddSingleton<IRegistrySettings, FabricConfigurationSettings>()
                .AddSingleton<IPolicyFactory, PolicyFactory>()
                .AddSingleton<IStepFactory, StepFactory>()
                .AddSingleton<IConfigFactory<Processor>, ConfigFactory<Processor>>()
                .AddSingleton<IConfigFactory<ClientConfig>, ConfigFactory<ClientConfig>>()
                .AddSingleton<IRegistry, Registry>()
                .AddScoped<IEngine, Engine>();
        }
    }
}