using CoreService.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoreService.Simulation.Steps
{
    public class RequestStep : IStep
    {
        /// <summary>
        /// The request url.
        /// </summary>
        [JsonProperty("url")]
        [JsonRequired]
        public string Url { get; set; }


        public async Task<ExecutionStatus> ExecuteAsync()
        {
            // TODO: if dealing with responses >50MB, implement streaming

            // TODO: add caller identifier to request

            AdaptableRequest request = new AdaptableRequest();
            // TODO: add payload

            // TODO: add cancellation token
            // TODO: use factory
            using (var client = new HttpClient())
            {
                HttpResponseMessage response =
                    await client.PostAsJsonAsync<AdaptableRequest>(Url, request);
            }


            // TODO: remove this
            return ExecutionStatus.Success;
        }


        /// <summary>
        /// Enumerable of supported Http methods in uppercase
        /// </summary>
        private readonly IEnumerable<string> supportedMethods = new List<HttpMethod>()
        {
            HttpMethod.Get,
            HttpMethod.Post,
            HttpMethod.Put,
            HttpMethod.Head,
            HttpMethod.Delete,
            HttpMethod.Trace,
            HttpMethod.Options
        }.Select(m => m.Method.ToUpperInvariant());
    }
}
