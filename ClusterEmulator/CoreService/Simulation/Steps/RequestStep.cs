using CoreService.Model;
using Newtonsoft.Json;
using Polly;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CoreService.Simulation.Steps
{
    public class RequestStep : IStep
    {
        /// <summary>
        /// Whether the request should reuse HttpClient instances or not.
        /// </summary>
        /// <remarks>
        /// Reusing Http clients prevents creation of new connections on new sockets for every request.
        /// </remarks>
        [JsonProperty("reuseSockets")]
        [JsonRequired]
        public bool ReuseHttpClient { get; set; }


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


        /// <summary>
        /// Executes the action defined by the step.
        /// </summary>
        /// <returns>A <see cref="ExecutionStatus"/> value.</returns>
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
            
            // TODO: add cancellation token

            // TODO: add optional true async vs await

            if (ReuseHttpClient)
            {
                // TODO: use correct name and make actual request
                HttpClient client = clientFactory.CreateClient("Todo");

                // TODO: refactor
                HttpResponseMessage response =
                       await client.PostAsJsonAsync(Url, request);
            }
            else
            {
                // TODO: add policies and make actual request
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseAddessPath);
                    foreach ((var key, var value) in headers)
                    {
                        client.DefaultRequestHeaders.Add(key, value);
                    }

                    if (policies is null)
                    {
                        // TODO: refactor
                        HttpResponseMessage response =
                            await client.PostAsJsonAsync(Url, request);
                    }
                    else
                    {
                        // TODO: refactor
                        // TODO: should this support local cancellation?
                        HttpResponseMessage response =
                            await policies.ExecuteAsync(
                                async ct => await client.PostAsJsonAsync(Url, request, ct),
                                CancellationToken.None);
                    }
                }
            }

            return await Task.FromResult(ExecutionStatus.Success);
        }


        /// <summary>
        /// Configures the request step for resolving  http clients from a client factory.
        /// </summary>
        /// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> instance.</param>
        public void Configure(IHttpClientFactory httpClientFactory)
        {
            if (configured)
            {
                throw new InvalidOperationException("The step is already configured");
            }

            if (!ReuseHttpClient)
            {
                throw new InvalidOperationException("This step cannot use http client factory");
            }

            clientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            configured = true;
        }


        /// <summary>
        /// Configures the request step for managment of the http client lifetime.
        /// </summary>
        /// <param name="requestPolicies">The wrapped policies to apply to all requests.</param>
        /// <param name="clientBaseAddress">The base address for the http client.</param>
        /// <param name="clientHeaders">Headers to send with all requests.</param>
        public void Configure(PolicyWrap requestPolicies, string clientBaseAddress, IDictionary<string, string> clientHeaders)
        {
            if (configured)
            {
                throw new InvalidOperationException("The step is already configured");
            }

            if (ReuseHttpClient)
            {
                throw new InvalidOperationException("This step must use http client factory");
            }

            if (string.IsNullOrWhiteSpace(baseAddessPath))
            {
                throw new ArgumentNullException(nameof(clientBaseAddress));
            }

            baseAddessPath = clientBaseAddress;
            policies = requestPolicies ?? throw new ArgumentNullException(nameof(requestPolicies));
            headers = clientHeaders ?? throw new ArgumentNullException(nameof(clientHeaders));
            configured = true;
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


        private bool configured = false;


        private PolicyWrap policies;


        private string baseAddessPath;


        private IDictionary<string, string> headers;


        private IHttpClientFactory clientFactory;


        /// <summary>
        /// Enumerable of supported Http methods in uppercase
        /// </summary>
        [JsonIgnore]
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
