using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Fabric;
using System.Text.RegularExpressions;
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

        private static readonly Regex UrlMatch = new Regex("\"(http:.+)\"");

        private static string randomServiceUrl;

        private static string lookupServiceUrl;


        private static string GetUrl(string applicationName, string serviceName)
        {
            Task<ResolvedServicePartition> task =
                Resolver.ResolveAsync(new Uri($"fabric:/{applicationName}/{serviceName}"), new ServicePartitionKey(), CancellationToken.None);
            ResolvedServicePartition partition = task.Result;

            // Resolve a random endpoint for the service
            ResolvedServiceEndpoint endpoint = partition.GetEndpoint();

            // Partition stores the endpoint address in a strange JSON format
            Match match = UrlMatch.Match(endpoint.Address);

            // Extract JSON escape characters
            string url = match.Groups[1]?.Value?.Replace("\\", string.Empty);
            ServiceEventSource.Current.Message(
                $"Resolved '{url}' from '{partition.Endpoints.Count}' endpoints for 'fabric:/{applicationName}/{serviceName}' from partition '{partition.Info.Id}'");
            return url;
        }
    }
}
