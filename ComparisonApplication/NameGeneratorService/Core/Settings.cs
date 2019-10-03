using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace NameGeneratorService.Core
{
    /// <summary>
    /// Provides access to common application settings
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// The client name for accessing the random number generation API
        /// </summary>
        public const string RandomApiClientName = "randomApi";


        /// <summary>
        /// The client name for accessing the name id lookup API
        /// </summary>
        public const string NameLookupApiClientName = "nameLookupApi";


        /// <summary>
        /// The base path of the random number generation service
        /// </summary>
        public static string RandomServiceBaseUrl
        {
            get
            {
                if (randomServiceUrl is null)
                {
                    randomServiceUrl = GetUrl("ComparisonApplication", "RandomGeneratorService");
                }

                return randomServiceUrl;
            }
        }

        /// <summary>
        /// The base path of the random number generation service
        /// </summary>
        public static string LookupServiceBaseUrl
        {
            get
            {
                if (lookupServiceUrl is null)
                {
                    lookupServiceUrl = GetUrl("ComparisonApplication", "NameLookupService");
                }

                return lookupServiceUrl;
            }
        }

        private static ServicePartitionResolver Resolver { get; } = ServicePartitionResolver.GetDefault();

        private static string randomServiceUrl;

        private static string lookupServiceUrl;


        private static string GetUrl(string applicationName, string serviceName)
        {
            Task<ResolvedServicePartition> task =
                Resolver.ResolveAsync(new Uri($"fabric:/{applicationName}/{serviceName}"), new ServicePartitionKey(), CancellationToken.None);
            ResolvedServicePartition partition = task.Result;

            // Partition stores the endpoint address in a strange JSON format so this extracts it
            JToken address = JToken.Parse(partition.GetEndpoint().Address);
            string url = address.First.First.First.First.Value<string>();

            ServiceEventSource.Current.Message($"Resolved url '{url}' for 'fabric:/{applicationName}/{serviceName}' from partition '{partition.Info.Id}'");
            return url;
        }
    }
}
