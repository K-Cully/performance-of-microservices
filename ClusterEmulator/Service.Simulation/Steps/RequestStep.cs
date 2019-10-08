using ClusterEmulator.Service.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ClusterEmulator.Service.Simulation.Steps
{
    public class RequestStep : SimulationStep, IRequestStep
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
        /// The optional id to use for cache key isolation
        /// </summary>
        [JsonProperty("cacheId")]
        public string CacheId { get; set; }


        /// <summary>
        /// The optional uniqueness factor to to use for cache key isolation
        /// </summary>
        [JsonProperty("cacheKeyUniqueness")]
        public int CacheUniqueness { get; set; }


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
        public async override Task<ExecutionStatus> ExecuteAsync()
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

            if (string.IsNullOrWhiteSpace(Method) || !supportedMethodNames.Contains(Method, StringComparer.OrdinalIgnoreCase))
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
                ExecutionStatus status = ExecutionStatus.Fail;
                Guid disposalId = Guid.NewGuid();
                try
                {
                    Func<CancellationToken, Task<HttpResponseMessage>> request = GetRequestAction(client, disposalId);
                    if (ReuseHttpMessageHandler) // Policies are already part of the request pipeline
                    {
                        status = await HandleRequestAsync(request, disposalId).ConfigureAwait(false);
                    }
                    else
                    {
                        // Policies need to be registered in the request pipeline
                        Func<CancellationToken, Task<HttpResponseMessage>> combinedAction;
                        if (policy is null)
                        {
                            combinedAction = token => request(token);
                        }
                        else
                        {
                            combinedAction = token => policy.ExecuteAsync(
                                (context, cancelationToken) => request(cancelationToken), Context, token);
                        }

                        status = await HandleRequestAsync(combinedAction, disposalId).ConfigureAwait(false);
                    }
                }
                finally
                {
                    DisposePending(disposalId);
                }

                return status;
            }
        }


        /// <summary>
        /// Disposes of any disposable objects created during http client creation or response
        /// </summary>
        /// <param name="disposalKey">The id of the request for object disposal.</param>
        private void DisposePending(Guid disposalKey)
        {
            if (pendingDisposals.TryGetValue(disposalKey, out IList<IDisposable> pending))
            {
                if (pending != null)
                {
                    foreach (IDisposable disposable in pending)
                    {
                        disposable.Dispose();
                    }
                }

                pendingDisposals.Remove(disposalKey);
                return;
            }
        }


        /// <summary>
        /// Schedules an object for delayed disposal.
        /// </summary>
        /// <param name="disposalKey">The id of the request for thread safe object disposal.</param>
        /// <param name="disposable">The object for delayed disposal.</param>
        private void ScheduleDisposal(Guid disposalKey, IDisposable disposable)
        {
            if (pendingDisposals.TryGetValue(disposalKey, out IList<IDisposable> pending) && pending != null)
            {
                pending.Add(disposable);
                return;
            }

            pendingDisposals[disposalKey] = new List<IDisposable> { disposable };
        }


        /// <summary>
        /// Handles request execution for asynchronous and non-asynchronous workloads
        /// </summary>
        /// <param name="request">The request to execute</param>
        /// <param name="disposalId">A unique request identifier for object disposal.</param>
        /// <returns>The execution status</returns>
        private async Task<ExecutionStatus> HandleRequestAsync(Func<CancellationToken, Task<HttpResponseMessage>> request, Guid disposalId)
        {
            if (Asynchrounous)
            {
                SendRequest(ExecuteRequestAsync(request, CancellationToken.None));
                return ExecutionStatus.Success;
            }
            else
            {
                HttpResponseMessage response = await ExecuteRequestAsync(request, CancellationToken.None);
                if (response is null)
                {
                    return ExecutionStatus.Fail;
                }

                if (!ReuseHttpMessageHandler || !response.Headers.Contains("policyCached"))
                {
                    // Only dispose message if it is not cached in the http factory pipeline
                    ScheduleDisposal(disposalId, response);
                }

                Logger.LogInformation("{Id}: Retrieved response {StatusCode}", CacheId, response.StatusCode);
                return response.IsSuccessStatusCode ? ExecutionStatus.Success : ExecutionStatus.Fail;
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
                Logger.LogWarning(ex, "{Id}: The operation timed out", CacheId);
            }
            catch (BrokenCircuitException ex)
            {
                Logger.LogWarning(ex, "{Id}: Circuit is broken", CacheId);
            }
            catch (Exception ex)
                when (ex is OperationCanceledException
                || ex.InnerException is OperationCanceledException)
            {
                Logger.LogWarning(ex, "{Id}: The operation was cancelled", CacheId);
            }

            return null;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0067:Dispose objects before losing scope", Justification = "Object does not need to be explicitly disposed")]
        private Func<CancellationToken, Task<HttpResponseMessage>> GetRequestAction(HttpClient client, Guid disposalId)
        {
            var method = new HttpMethod(Method);
            if (!supportedMethods.Contains(method))
            {
                Logger.LogCritical("{HttpMethod} is not supported", Method);
                throw new InvalidOperationException($"{Method} is not supported");
            }

            Logger.LogDebug("{Id}: Retrieving action for {HttpMethod}", CacheId, Method);

            HttpContent content = null;
            if (method == HttpMethod.Put
                || method == HttpMethod.Post)
            {
                var adaptableRequest = GenerateRequest();
                content = new ObjectContent<AdaptableRequest>(adaptableRequest, new JsonMediaTypeFormatter(), mediaType: (MediaTypeHeaderValue)null);
            }

            return token =>
            {
                var request = new HttpRequestMessage(method, Path);
                ScheduleDisposal(disposalId, request);
                if (ReuseHttpMessageHandler)
                {
                    // Only set the context in the request when policies are registered with HttpClientFactory
                    request.SetPolicyExecutionContext(Context);
                }

                request.Content = content;
                return client.SendAsync(request, token);
            };
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
                Logger.LogError("{Id}: Asynchronous request faulted", CacheId);
                task.Exception.Handle(ex =>
                {
                    // Log and set all exceptions as handled to avoid rethrowing
                    Logger.LogError(ex, "{Id}: Exception returned from asynchronous request", CacheId);
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
            Logger.LogInformation("{Id}: Client factory configured successfully", CacheId);
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
            Logger.LogInformation("{Id}: Client factory and policies configured successfully", CacheId);
        }


        /// <summary>
        /// Retrieves a policy context with a cache key
        /// </summary>
        [JsonIgnore]
        private Context Context => new Context($"{CacheId ?? "NULL"}-{CacheKeyNumber}-{Method}");


        /// <summary>
        /// Retrieves a value to signify cache key uniqueness for the request
        /// </summary>
        /// <remarks>
        /// This avoids negatives and wraparound issues using the followin approach:
        /// If cache uniqueness is less than 1, 0 is returned.
        /// If cache uniqueness is int.MaxValue, a random integer in the inclusive range 1 to (int.MaxValue - 1) is returned.
        /// Otherwise, a random integer in the inclusive range 1 to cache uniqueness is returned.
        /// </remarks>
        [JsonIgnore]
        private int CacheKeyNumber => CacheUniqueness < 1 ? 0 :
            random.Next(1, CacheUniqueness == int.MaxValue ? int.MaxValue : CacheUniqueness + 1);


        [JsonIgnore]
        private bool configured = false;


        [JsonIgnore]
        private IAsyncPolicy<HttpResponseMessage> policy;


        [JsonIgnore]
        private IHttpClientFactory clientFactory;


        [JsonIgnore]
        private readonly Random random = new Random();


        /// <summary>
        /// A list of objects that need to be disposed once all request operations have completed
        /// </summary>
        /// <remarks>Allows policies (e.g. Cache) to access http communication objects.</remarks>
        [JsonIgnore]
        private readonly IDictionary<Guid, IList<IDisposable>> pendingDisposals = new Dictionary<Guid, IList<IDisposable>>();


        /// <summary>
        /// Set of supported Http methods in uppercase
        /// </summary>
        [JsonIgnore]
        private readonly ISet<string> supportedMethodNames =
            new HashSet<string>(httpMethods.Select(m => m.Method.ToUpperInvariant()));


        /// <summary>
        /// Set of supported Http methods
        /// </summary>
        [JsonIgnore]
        private readonly ISet<HttpMethod> supportedMethods =
            new HashSet<HttpMethod>(httpMethods);


        [JsonIgnore]
        private static readonly IEnumerable<HttpMethod> httpMethods =
            new List<HttpMethod>()
            {
                HttpMethod.Delete,
                HttpMethod.Get,
                HttpMethod.Head,
                HttpMethod.Options,
                HttpMethod.Post,
                HttpMethod.Put,
                HttpMethod.Trace
            };
    }
}
