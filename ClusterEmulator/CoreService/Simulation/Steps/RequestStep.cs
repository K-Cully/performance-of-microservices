using CoreService.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoreService.Simulation.Steps
{
    public class RequestStep : IStep
    {
        /// <summary>
        /// The http protocol.
        /// </summary>
        [JsonProperty("method")]
        [JsonRequired]
        public string Method { get; set; }


        /// <summary>
        /// The request url.
        /// </summary>
        [JsonProperty("url")]
        [JsonRequired]
        public string Url { get; set; }


        /// <summary>
        /// The request payload size.
        /// </summary>
        [JsonProperty("size")]
        [JsonRequired]
        [Range(0, int.MaxValue, ErrorMessage = "size must be greater than 0")]
        public int PayloadSize { get; set; }


        public async Task<ExecutionStatus> ExecuteAsync()
        {
            // Policy -> name and details

            // HttpClient -> name, policies and details (host and headers?)

            // Request -> if reusable_http_client_name, path and request else host, path, headers, policy and request


            // TODO: if dealing with responses >50MB, implement streaming

            // TODO: unify sizes to be in bytes

            if (!UrlIsValid())
            {
                throw new InvalidOperationException("url must be a valid absolute URI");
            }

            if (string.IsNullOrWhiteSpace(Method) || !supportedMethods.Contains(Method, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("url must be a valid http protocol");
            }

            if (PayloadSize < 0)
            {
                throw new InvalidOperationException("size must be greater than 0");
            }

            // TODO: add caller identifier to request

            AdaptableRequest request = new AdaptableRequest();
            // TODO: add payload

            // Policy.Wrap(fallback, cache, retry, breaker, timeout, bulkhead)

            // TODO: add cancellation token
            // TODO: use factory with optional "reuse sockets" flag
            //using (var client = new HttpClient())
            //{
            //    HttpResponseMessage response =
            //       await client.PostAsJsonAsync<AdaptableRequest>(Url, request);
            //}

            // TODO: add configuration of Handler lifetime

            // TODO: remove this


            // Example usage of policy
            //Policy timeoutPolicy = Policy.TimeoutAsync(30, TimeoutStrategy.Optimistic);

            //HttpResponseMessage httpResponse = await timeoutPolicy
            //    .ExecuteAsync(
            //        async ct => await httpClient.GetAsync(requestEndpoint, ct),
            //        userCancellationSource.Token
            //        );

            return await Task.FromResult(ExecutionStatus.Success);
        }


        private bool UrlIsValid()
        {
            bool valid = false;
            if (!string.IsNullOrWhiteSpace(Url))
            {
                try
                {
                    var uri = new UriBuilder(Url).Uri;
                    valid = uri.IsAbsoluteUri;
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (UriFormatException)
                {
                    valid = false;
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }

            return valid;
        }


        /// <summary>
        /// Enumerable of supported Http methods in uppercase
        /// </summary>
        private readonly IEnumerable<string> supportedMethods = new List<HttpMethod>()
        {
            HttpMethod.Delete,
            HttpMethod.Get,
            HttpMethod.Head,
            HttpMethod.Options,
            HttpMethod.Post,
            HttpMethod.Put,
            HttpMethod.Trace,
        }.Select(m => m.Method.ToUpperInvariant());
    }
}
