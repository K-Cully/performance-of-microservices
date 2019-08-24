using System.Collections.Generic;
using System.Fabric;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using CoreService.Simulation.Steps;
using CoreService.Simulation.Core;
using CoreService.Simulation.Processors;
using CoreService.Simulation.HttpClientConfiguration;
using Serilog.Core.Enrichers;
using System;
using Serilog;

namespace CoreService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class CoreService : StatelessService
    {
        private ILogger SharedLogger { get; }


        private ILogger Logger { get; }


        /// <summary>
        /// Creates a new instance of <see cref="CoreService"/>
        /// </summary>
        /// <param name="context">The stateless service context to initialize the service with.</param>
        /// <param name="serilogger">The <see cref="ILogger"/> instance to register with the ASP.Net Core logging pipeline</param>
        public CoreService(StatelessServiceContext context, ILogger serilogger)
            : base(context)
        {
            SharedLogger = serilogger ?? throw new ArgumentNullException(nameof(serilogger));

            // Create log enrichers with service execution context
            PropertyEnricher[] properties = new PropertyEnricher[]
            {
                new PropertyEnricher("ServiceTypeName", context.ServiceTypeName),
                new PropertyEnricher("ServiceName", context.ServiceName),
                new PropertyEnricher("PartitionId", context.PartitionId),
                new PropertyEnricher("InstanceId", context.ReplicaOrInstanceId),
            };

            // Add service context to logger
            Logger = serilogger.ForContext(properties);
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
                        // TODO: remove old logging
                        // ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        // TODO: utilize structured log data (logging is not created here yet)
                        SharedLogger.Information($"Starting Kestrel on {url}");

                        // TODO: register logger

                        return new WebHostBuilder()
                                    .UseKestrel()
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton<StatelessServiceContext>(serviceContext)
                                            .AddSingleton<IRegistry>(new Registry(
                                                serviceContext.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings,
                                                new StepFactory(),
                                                new ConfigFactory<Processor>(),
                                                new PolicyFactory(),
                                                new ConfigFactory<ClientConfig>()))
                                            .AddScoped<IEngine, Engine>())
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseSerilog(Logger, dispose: true) // TODO: confirm the instance is copied and requires disposal
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url)
                                    .Build();
                    }))
            };
        }
    }
}
