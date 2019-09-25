using ClusterEmulator.Service.Shared.Configuration;
using ClusterEmulator.Service.Simulation.Core;
using ClusterEmulator.Service.Simulation.HttpClientConfiguration;
using ClusterEmulator.Service.Simulation.Processors;
using ClusterEmulator.Service.Simulation.Steps;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Serilog;
using Serilog.Core;
using Serilog.Core.Enrichers;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;

namespace CoreService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class CoreService : StatelessService
    {
        private ILogger Log { get; }


        /// <summary>
        /// Creates a new instance of <see cref="CoreService"/>
        /// </summary>
        /// <param name="context">The stateless service context to initialize the service with.</param>
        /// <param name="logger">The <see cref="ILogger"/> instance to register with the ASP.Net Core logging pipeline</param>
        public CoreService(StatelessServiceContext context, ILogger logger)
            : base(context)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));

            // Create log enrichers with service execution context
            ILogEventEnricher[] properties = new ILogEventEnricher[]
            {
                new PropertyEnricher("ServiceTypeName", context.ServiceTypeName),
                new PropertyEnricher("ServiceName", context.ServiceName),
                new PropertyEnricher("PartitionId", context.PartitionId),
                new PropertyEnricher("InstanceId", context.ReplicaOrInstanceId)
            };

            // Add service context to logger
            Log = logger.ForContext(properties);
        }


        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        Log.Information("Starting Kestrel on {Endpoint}", url);

                        return new WebHostBuilder()
                                    .UseKestrel()
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton(serviceContext)
                                            .AddSingleton(serviceContext.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings)
                                            .AddSingleton<IRegistrySettings, FabricConfigurationSettings>()
                                            .AddSingleton<IPolicyFactory, PolicyFactory>()
                                            .AddSingleton<IStepFactory, StepFactory>()
                                            .AddSingleton<IConfigFactory<Processor>, ConfigFactory<Processor>>()
                                            .AddSingleton<IConfigFactory<ClientConfig>, ConfigFactory<ClientConfig>>()
                                            .AddSingleton<IRegistry, Registry>()
                                            .AddScoped<IEngine, Engine>())
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseSerilog(Log, dispose: true)
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url)
                                    .Build();
                    }))
            };
        }
    }
}