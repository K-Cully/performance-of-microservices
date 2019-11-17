using ClusterEmulator.ApplicationInsights.Telemetry;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.ServiceFabric.Services.Runtime;
using Serilog;
using Serilog.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace EmulationService
{
    internal static class Program
    {
        private const string ServiceTypeName = "EmulationServiceType";


        /// <summary>
        /// App settings for use in log configuration
        /// </summary>
        private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();


        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                Logger log = new LoggerConfiguration()
                                .ReadFrom.Configuration(Configuration)
                                .CreateLogger();

                // Create service instance
                ServiceRuntime.RegisterServiceAsync(ServiceTypeName,
                    context => new EmulationService(context, log)).GetAwaiter().GetResult();

                Log.Information("Service registered - {ProcessId}, {ServiceId}",
                    Process.GetCurrentProcess().Id, ServiceTypeName);

                // Prevents this host process from terminating so services keep running. 
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Failed to initialize service {ServiceId}", ServiceTypeName);
                throw;
            }
            finally
            {
                // Clean up logger if process terminates
                Log.CloseAndFlush();
            }
        }
    }
}
