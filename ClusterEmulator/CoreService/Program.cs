using Microsoft.ServiceFabric.Services.Runtime;
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

                ServiceRuntime.RegisterServiceAsync("CoreServiceType",
                    context => new CoreService(context)).GetAwaiter().GetResult();

                // TODO: Update EventSource logging
                // ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(CoreService).Name);

                // Prevents this host process from terminating so services keeps running. 
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                // TODO: Update EventSource logging
                // ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
