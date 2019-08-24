using Microsoft.ServiceFabric.Services.Runtime;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CoreService
{
    internal static class Program
    {
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


                // TODO: create other sinks
                // TODO: can this being static cause issues?

                // TODO: restrit debug logging to dev environment
                // Create SeriLog debug logger
                //Log.Logger = new LoggerConfiguration().WriteTo.Debug().CreateLogger();

                Log.Logger = new LoggerConfiguration()
                                .MinimumLevel.Debug()
                                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                                .Enrich.FromLogContext()
                                .WriteTo.Debug()
                                .CreateLogger();

                // Create service instance
                ServiceRuntime.RegisterServiceAsync("CoreServiceType",
                    context => new CoreService(context, Log.Logger)).GetAwaiter().GetResult();

                // TODO: Update EventSource logging?
                // ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(CoreService).Name);
                Log.Information($"Service registered - {Process.GetCurrentProcess().Id}, {typeof(CoreService).Name}");

                // Prevents this host process from terminating so services keeps running. 
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                // TODO: Update EventSource logging
                // ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
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
