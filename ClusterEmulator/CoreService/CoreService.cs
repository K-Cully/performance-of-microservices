using ClusterEmulator.Service.Shared.Extensions;
using ClusterEmulator.Service.Shared.Telemetry;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Serilog;
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

            // Add service context to logger
            Log = logger.ForContext(new StatelessServiceEnricher(context));
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
                                            .AddSimulationEngine(serviceContext))
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