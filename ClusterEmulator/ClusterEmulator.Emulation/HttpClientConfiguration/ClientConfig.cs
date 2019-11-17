using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClusterEmulator.Emulation.HttpClientConfiguration
{
    /// <summary>
    /// Defines configuration for a HttpClient instance.
    /// </summary>
    [JsonObject]
    public class ClientConfig
    {
        /// <summary>
        /// The base address of the URI for all requests
        /// </summary>
        [JsonProperty("baseAddress")]
        [JsonRequired]
        public string BaseAddress { get; set; }


        /// <summary>
        /// The list of policies to apply to all requests
        /// </summary>
        [JsonProperty("policies")]
        [JsonRequired]
        public IEnumerable<string> Policies { get; set; }


        /// <summary>
        /// Headers to send with all requests
        /// </summary>
        [JsonProperty("headers")]
        public IDictionary<string, string> RequestHeaders { get; set; }
    }
}
