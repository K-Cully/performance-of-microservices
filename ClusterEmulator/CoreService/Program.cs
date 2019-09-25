using ClusterEmulator.Service.Shared.Telemetry;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.ServiceFabric.Services.Runtime;
using Serilog;
using Serilog.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CoreService
{
    internal static class Program
    {
        private const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";


        /// <summary>
        /// App settings for use in log configuration
        /// </summary>
        private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable(ASPNETCORE_ENVIRONMENT) ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();


        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                var telemetry = new TelemetryConfiguration("");
                Logger log = new LoggerConfiguration()
                            .ReadFrom.Configuration(Configuration)
                            .Enrich.FromLogContext()
                            .WriteTo.ApplicationInsights(telemetry, new OperationTelemetryConverter())
                            .CreateLogger();

                // Create service instance
                ServiceRuntime.RegisterServiceAsync("CoreServiceType",
                    context => new CoreService(context, log)).GetAwaiter().GetResult();

                Log.Information($"Service registered - {Process.GetCurrentProcess().Id}, {typeof(CoreService).Name}");

                // Prevents this host process from terminating so services keeps running. 
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Failed to initialize service");
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
