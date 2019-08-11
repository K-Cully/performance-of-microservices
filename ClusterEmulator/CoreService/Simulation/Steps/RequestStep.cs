using CoreService.Model;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CoreService.Simulation.Steps
{
    public class RequestStep : IStep, IRequestStep
    {
        // TODO: add UTs
        // TODO: flesh out model


        /// <summary>
        /// Whether the request should be truely asynchronous (fire and forget) or should await responses.
        /// </summary>
        [JsonProperty("trueAsync")]
        [JsonRequired]
        public bool Asynchrounous { get; set; }


        /// <summary>
        /// The name of the client to use
        /// </summary>
        [JsonProperty("client")]
        [JsonRequired]
        public string ClientName { get; set; }


        /// <summary>
        /// The http protocol.
        /// </summary>
        [JsonProperty("method")]
        [JsonRequired]
        public string Method { get; set; }


        /// <summary>
        /// The request path.
        /// </summary>
        [JsonProperty("path")]
        [JsonRequired]
        public string Path { get; set; }


        /// <summary>
        /// The request payload size.
        /// </summary>
        [JsonProperty("size")]
        [JsonRequired]
        [Range(0, int.MaxValue, ErrorMessage = "size cannot be negative")]
        public int PayloadSize { get; set; }


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
        /// Gets a value indicating if the request step is configured or not.
        /// </summary>
        [JsonIgnore]
        public bool Configured { get { return configured; } }


        /// <summary>
        /// Executes the action defined by the step.
        /// </summary>
        /// <returns>A <see cref="ExecutionStatus"/> value.</returns>
        public async Task<ExecutionStatus> ExecuteAsync()
        {
            // TODO: Add logging throughout

            if (string.IsNullOrWhiteSpace(ClientName))
            {
                throw new InvalidOperationException("client must be set");
            }

            if (string.IsNullOrWhiteSpace(Path) || !Uri.TryCreate(Path, UriKind.Relative, out _))
            {
                throw new InvalidOperationException("path must be a relative URI");
            }

            if (string.IsNullOrWhiteSpace(Method) || !supportedMethods.Contains(Method, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("url must be a valid http protocol");
            }

            if (PayloadSize < 0)
            {
                throw new InvalidOperationException("size cannot be negative");
            }

            if (!configured)
            {
                throw new InvalidOperationException("Http client is not configured");
            }

            // TODO: handle polly exceptions, eg. timeout

            // TODO: unify sizes to be in bytes

            // TODO: add caller identifier

            // Note: Will need to be updated to use streaming if support for responses >50MB is ever required.

            Func<CancellationToken, Task<HttpResponseMessage>> request;
            if (ReuseHttpClient)
            {
                HttpClient client = clientFactory.CreateClient(ClientName);
                if (client is null)
                {
                    return ExecutionStatus.Unexpected;
                }

                request = GetRequestAction(client);
                if (Asynchrounous)
                {
                    SendRequest(ExecuteRequestAsync(request, CancellationToken.None));
                }
                else
                {
                    using (HttpResponseMessage response = await ExecuteRequestAsync(request, CancellationToken.None))
                    {
                        return response.IsSuccessStatusCode ? ExecutionStatus.Success : ExecutionStatus.Fail;
                    }
                }
            }
            else
            {
                using (var client = clientFactory.CreateClient(ClientName))
                {
                    if (client is null)
                    {
                        // TODO: add UT
                        return ExecutionStatus.Unexpected;
                    }

                    request = GetRequestAction(client);
                    Func<CancellationToken, Task<HttpResponseMessage>> combinedAction;
                    if (policy is null)
                    {
                        combinedAction = token => request(token);
                    }
                    else
                    {
                        combinedAction = token => policy.ExecuteAsync(
                            ct => request(ct), token);
                    }

                    if (Asynchrounous)
                    {
                        SendRequest(ExecuteRequestAsync(combinedAction, CancellationToken.None));
                    }
                    else
                    {
                        using (HttpResponseMessage response =
                            await ExecuteRequestAsync(combinedAction, CancellationToken.None))
                        {
                            return response.IsSuccessStatusCode ? ExecutionStatus.Success : ExecutionStatus.Fail;
                        }
                    }
                }
            }

            return ExecutionStatus.Success;
        }


        private Func<CancellationToken, Task<HttpResponseMessage>> GetRequestAction(HttpClient client)
        {
            // TODO: refactor to remove duplication

            var method = new HttpMethod(Method);

            if (method == HttpMethod.Delete)
            {
                return token => client.DeleteAsync(Path, token);
            }
            if (method == HttpMethod.Get)
            {
                return token => client.GetAsync(Path, token);
            }
            if (method == HttpMethod.Head)
            {
                return token =>
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Head, Path))
                        return client.SendAsync(request, token);
                };
            }
            if (method == HttpMethod.Options)
            {
                return token =>
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Options, Path))
                        return client.SendAsync(request, token);
                };
            }
            if (method == HttpMethod.Post)
            {
                // TODO: add payload
                // TODO: add caller identifier to request
                var request = new AdaptableRequest();
                return token => client.PostAsJsonAsync(Path, request, token);
            }
            if (method == HttpMethod.Put)
            {
                // TODO: add payload
                // TODO: add caller identifier to request
                var request = new AdaptableRequest();
                return token => client.PutAsJsonAsync(Path, request, token);
            }
            if (method == HttpMethod.Trace)
            {
                return token =>
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Trace, Path))
                        return client.SendAsync(request, token);
                };
            }

            throw new InvalidOperationException($"{Method} is not supported");
        }


        /// <summary>
        /// Executes the request following the async-await pattern, freeing the current thread back to the thread pool.
        /// </summary>
        /// <param name="action">The request action.</param>
        /// <param name="token">The local cacellation token.</param>
        /// <returns>The response message from the request.</returns>
        private async Task<HttpResponseMessage> ExecuteRequestAsync(
            Func<CancellationToken, Task<HttpResponseMessage>> action, CancellationToken token)
        {
            return await action(token).ConfigureAwait(false);
        }


        /// <summary>
        /// Sends the request asynchronously with error handling if the task faults.
        /// </summary>
        /// <param name="requestTask">The request task to execute asynchronously.</param>
        private void SendRequest(Task<HttpResponseMessage> requestTask)
        {
            requestTask.ContinueWith(t =>
            {
                //t?.Result?.Content?.Dispose();
                // TODO: Log error
                string error = t?.Exception?.Message;

            }, TaskContinuationOptions.OnlyOnFaulted);
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
        /// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> instance.</param>
        /// <param name="requestPolicy">The policy or wrapped policies to apply to requests.</param>
        public void Configure(IHttpClientFactory httpClientFactory, IAsyncPolicy requestPolicy)
        {
            if (configured)
            {
                throw new InvalidOperationException("The step is already configured");
            }

            if (ReuseHttpClient)
            {
                throw new InvalidOperationException("This step must use http client factory");
            }

            clientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            policy = requestPolicy.AsAsyncPolicy<HttpResponseMessage>();
            configured = true;
        }


        [JsonIgnore]
        private bool configured = false;


        [JsonIgnore]
        private IAsyncPolicy<HttpResponseMessage> policy;


        [JsonIgnore]
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
