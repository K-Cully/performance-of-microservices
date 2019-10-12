using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.ServiceFabric;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using NameLookupService.Core;
using System.Collections.Generic;
using System.Fabric;
using System.IO;

namespace NameLookupService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class NameLookupService : StatelessService
    {
        public NameLookupService(StatelessServiceContext context)
            : base(context)
        { }

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
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");
                        var aiOptions = new ApplicationInsightsServiceOptions { EnableAdaptiveSampling = false };

                        return new WebHostBuilder()
                                    .UseKestrel()
                                    .ConfigureLogging((hostingContext, logging) =>
                                    {
                                        logging.SetMinimumLevel(LogLevel.Information);
                                        logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information)
                                            .AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
                                    })
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton<ITelemetryInitializer>((serviceProvider) =>
                                                FabricTelemetryInitializerExtension.CreateFabricTelemetryInitializer(serviceContext))
                                            .AddApplicationInsightsTelemetry(aiOptions)
                                            .AddSingleton(serviceContext)
                                            .AddSingleton<INameStore, NameStore>())
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url)
                                    .Build();
                    }))
            };
        }
    }
}
