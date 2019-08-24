using Microsoft.Extensions.Configuration;
using Microsoft.ServiceFabric.Services.Runtime;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CoreService
{
    internal static class Program
    {
        private const string Development = "Development";
        private const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";
        private const string LogTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";


        /// <summary>
        /// App settings for use in log configuration
        /// </summary>
        private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable(ASPNETCORE_ENVIRONMENT) ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();


        private static bool DevelopmentEnvironment => Configuration.GetValue(ASPNETCORE_ENVIRONMENT, string.Empty) == Development;


        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.


                // TODO: Investigate - can Log.Logger being static cause issues due to shared process?

                if (DevelopmentEnvironment)
                {
                    // Create Serilog debug logger
                    Log.Logger = new LoggerConfiguration()
                                    .ReadFrom.Configuration(Configuration)
                                    .Enrich.FromLogContext()
                                    .WriteTo.Debug(outputTemplate: LogTemplate)
                                    .CreateLogger();
                }
                else
                {
                    // TODO: create other sinks
                }

                // Create service instance
                ServiceRuntime.RegisterServiceAsync("CoreServiceType",
                    context => new CoreService(context, Log.Logger)).GetAwaiter().GetResult();

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
