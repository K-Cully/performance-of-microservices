using CoreService.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CoreService.Simulation.Steps
{
    public class RequestStep : IStep, IRequestStep
    {
        [JsonIgnore]
        private const int ChunkChars = 64;


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
        /// The request payload size in bytes.
        /// </summary>
        [JsonProperty("size")]
        [JsonRequired]
        [Range(0, int.MaxValue, ErrorMessage = "size cannot be negative")]
        public int PayloadSize { get; set; }


        /// <summary>
        /// Whether the request should reuse HttpMessageHandler instances or not.
        /// </summary>
        /// <remarks>
        /// Reusing Http Message Handlers prevents creation of new connections on new sockets for every request.
        /// </remarks>
        [JsonProperty("reuseSockets")]
        [JsonRequired]
        public bool ReuseHttpMessageHandler { get; set; }


        /// <summary>
        /// Gets a value indicating if the request step is configured or not.
        /// </summary>
        [JsonIgnore]
        public bool Configured { get { return configured; } }


        /// <summary>
        /// Executes the action defined by the step.
        /// </summary>
        /// <returns>A <see cref="ExecutionStatus"/> value.</returns>
        /// <remarks>Will need to be updated to use streaming if support for responses >50MB is ever required.</remarks>
        public async Task<ExecutionStatus> ExecuteAsync()
        {
            if (Logger is null)
            {
                throw new InvalidOperationException("Logger is not initialized");
            }

            if (string.IsNullOrWhiteSpace(ClientName))
            {
                Logger.LogCritical("{Property} value is not set", "client");
                throw new InvalidOperationException("client must be set");
            }

            if (string.IsNullOrWhiteSpace(Path) || !Uri.TryCreate(Path, UriKind.Relative, out _))
            {
                Logger.LogCritical("{Property} is not a relative URI", "path");
                throw new InvalidOperationException("path must be a relative URI");
            }

            if (string.IsNullOrWhiteSpace(Method) || !supportedMethods.Contains(Method, StringComparer.OrdinalIgnoreCase))
            {
                Logger.LogCritical("{Property} is not a valid http protocol", "method");
                throw new InvalidOperationException("method must be a valid http protocol");
            }

            if (PayloadSize < 0)
            {
                Logger.LogCritical("{Property} is negative", "size");
                throw new InvalidOperationException("size cannot be negative");
            }

            if (!configured)
            {
                Logger.LogCritical("The http client factory is not configured");
                throw new InvalidOperationException("Http client is not configured");
            }

            using (var client = clientFactory.CreateClient(ClientName) ??
                    throw new InvalidOperationException($"Client '{ClientName}' is null"))
            {
                Func<CancellationToken, Task<HttpResponseMessage>> request = GetRequestAction(client);
                if (ReuseHttpMessageHandler) // Policies are already part of the request pipeline
                {
                    return await HandleRequestAsync(request).ConfigureAwait(false);
                }

                // Policies need to be registered in the request pipeline
                Func<CancellationToken, Task<HttpResponseMessage>> combinedAction;
                if (policy is null)
                {
                    // TODO: figure out how to set context in this pipeline (https://nodogmablog.bryanhogan.net/2018/11/caching-in-polly-6-and-the-httpclientfactory/)
                    combinedAction = token => request(token);
                }
                else
                {
                    // TODO: set context correctly
                    combinedAction = token => policy.ExecuteAsync(
                        (context, cancelationToken) => request(cancelationToken), new Context("Foo"), token);
                }

                return await HandleRequestAsync(combinedAction).ConfigureAwait(false);
            }
        }


        /// <summary>
        /// Handles request execution for asynchronous and non-asynchronous workloads
        /// </summary>
        /// <param name="request">The request to execute</param>
        /// <returns>The execution status</returns>
        private async Task<ExecutionStatus> HandleRequestAsync(Func<CancellationToken, Task<HttpResponseMessage>> request)
        {
            if (Asynchrounous)
            {
                SendRequest(ExecuteRequestAsync(request, CancellationToken.None));
                return ExecutionStatus.Success;
            }
            else
            {
                using (HttpResponseMessage response =
                    await ExecuteRequestAsync(request, CancellationToken.None))
                {
                    if (response is null)
                    {
                        return ExecutionStatus.Fail;
                    }

                    Logger.LogInformation("Retrieved response {StatusCode}", response.StatusCode);
                    return response.IsSuccessStatusCode ? ExecutionStatus.Success : ExecutionStatus.Fail;
                }
            }
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
            try
            {
                return await action(token).ConfigureAwait(false);
            }
            catch (TimeoutRejectedException ex)
            {
                log.LogError(ex, "The operation timed out");
            }
            catch (Exception ex)
                when (ex is OperationCanceledException
                || ex.InnerException is OperationCanceledException)
            {
                log.LogError(ex, "The operation was cancelled");
            }

            return null;
        }


        private Func<CancellationToken, Task<HttpResponseMessage>> GetRequestAction(HttpClient client)
        {
            var method = new HttpMethod(Method);
            Logger.LogDebug("Retrieving action for {HttpMethod}", Method);

            if (method == HttpMethod.Head
                || method == HttpMethod.Options
                || method == HttpMethod.Trace)
            {
                return token =>
                {
                    using (var httpRequest = new HttpRequestMessage(method, Path))
                        return client.SendAsync(httpRequest, token);
                };
            }

            if (method == HttpMethod.Delete)
            {
                return token => client.DeleteAsync(Path, token);
            }

            if (method == HttpMethod.Get)
            {
                return token => client.GetAsync(Path, token);
            }

            var adaptableRequest = GenerateRequest();
            if (method == HttpMethod.Post)
            {
                return token => client.PostAsJsonAsync(Path, adaptableRequest, token);
            }

            if (method == HttpMethod.Put)
            {
                return token => client.PutAsJsonAsync(Path, adaptableRequest, token);
            }

            Logger.LogCritical("{HttpMethod} is not supported", Method);
            throw new InvalidOperationException($"{Method} is not supported");
        }


        /// <summary>
        /// Initializes a logger for the step instance.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        public void InitializeLogger(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        /// Generates a payload for requests which send a body.
        /// </summary>
        /// <returns>A <see cref="AdaptableRequest"/> instance of the configured size.</returns>
        private AdaptableRequest GenerateRequest()
        {
            var payload = new List<string>();

            if (PayloadSize > 0)
            {
                // Convert from byte count to char count
                var charCount = (PayloadSize / 2) + (PayloadSize % 2);
                while (charCount > 0)
                {
                    int chars = charCount < ChunkChars ? charCount : ChunkChars;
                    payload.Add(new string('r', chars));
                    charCount -= chars;
                }
            }

            return new AdaptableRequest()
            {
                Payload = payload
            };
        }


        /// <summary>
        /// Sends the request asynchronously with error handling if the task faults.
        /// </summary>
        /// <param name="requestTask">The request task to execute asynchronously.</param>
        private void SendRequest(Task<HttpResponseMessage> requestTask)
        {
            requestTask.ContinueWith(task =>
            {
                Logger.LogError("Asynchronous request faulted");
                task.Exception.Handle(ex =>
                {
                    // Log and set all exceptions as handled to avoid rethrowing
                    Logger.LogError(ex, "Exception returned from asynchronous request");
                    return true;
                });
            }, TaskContinuationOptions.OnlyOnFaulted);
        }


        /// <summary>
        /// Configures the request step for resolving  http clients from a client factory.
        /// </summary>
        /// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> instance.</param>
        public void Configure(IHttpClientFactory httpClientFactory)
        {
            if (Logger is null)
            {
                throw new InvalidOperationException("Logger is not initialized");
            }

            if (configured)
            {
                Logger.LogCritical("The client factory has already been configured");
                throw new InvalidOperationException("The step is already configured");
            }

            if (!ReuseHttpMessageHandler)
            {
                Logger.LogCritical("The step should resolve it's client from a custom IHttpClientFactory");
                throw new InvalidOperationException("This step cannot use the default http client factory");
            }

            clientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            configured = true;
            Logger.LogInformation("Client factory configured successfully");
        }


        /// <summary>
        /// Configures the request step for managment of the http client lifetime.
        /// </summary>
        /// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> instance.</param>
        /// <param name="requestPolicy">The policy or wrapped policies to apply to requests.</param>
        public void Configure(IHttpClientFactory httpClientFactory, IAsyncPolicy<HttpResponseMessage> requestPolicy)
        {
            if (Logger is null)
            {
                throw new InvalidOperationException("Logger is not initialized");
            }

            if (configured)
            {
                Logger.LogCritical("The client factory has already been configured");
                throw new InvalidOperationException("The step is already configured");
            }

            if (ReuseHttpMessageHandler)
            {
                Logger.LogCritical("The step should resolve it's client from the default IHttpClientFactory");
                throw new InvalidOperationException("This step must use the default http client factory");
            }

            clientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            policy = requestPolicy;
            configured = true;
            Logger.LogInformation("Client factory and policies configured successfully");
        }


        [JsonIgnore]
        private ILogger log;


        [JsonIgnore]
        private ILogger Logger { get => log; set => log = log ?? value; }


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
