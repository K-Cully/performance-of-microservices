using System.Collections.Generic;
using System.Fabric;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using CoreService.Simulation.Steps;
using CoreService.Simulation.Core;
using CoreService.Simulation.Processors;
using CoreService.Simulation.HttpClientConfiguration;

namespace CoreService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class CoreService : StatelessService
    {
        private ILogger Logger { get; }


        /// <summary>
        /// Creates a new instance of <see cref="CoreService"/>
        /// </summary>
        /// <param name="context">The stateless service context to initialize the service with.</param>
        public CoreService(StatelessServiceContext context)
            : base(context)
        {
            Logger = new LoggerFactory().CreateLogger<CoreService>();
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
                        // Logger.LogInformation($"Starting Kestrel on {url}");

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
                                    .ConfigureLogging(logging => ConfigureLogging(logging))
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url)
                                    .Build();
                    }))
            };
        }


        private void ConfigureLogging(ILoggingBuilder builder)
        {
            // TODO: remove once correct sinks are added

            // clear default logging providers
            // builder.ClearProviders();

            // add built-in providers manually, as needed
            builder.AddDebug();
            builder.AddEventSourceLogger();
        }
    }
}
