using Polly;
using System.Net.Http;

namespace CoreService.Simulation.Steps
{
    /// <summary>
    /// Interface for request steps requiring http factory configuration.
    /// </summary>
    public interface IRequestStep
    {
        /// <summary>
        /// The name of the client to use
        /// </summary>
        string ClientName { get; set; }


        /// <summary>
        /// Whether the request should reuse HttpMessageHandler instances or not.
        /// </summary>
        /// <remarks>
        /// Reusing Http Message Handlers prevents creation of new connections on new sockets for every request.
        /// </remarks>
        bool ReuseHttpMessageHandler { get; set; }


        /// <summary>
        /// Configures the request step for resolving  http clients from a client factory.
        /// </summary>
        /// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> instance.</param>
        void Configure(IHttpClientFactory httpClientFactory);


        /// <summary>
        /// Configures the request step for managment of the http client lifetime.
        /// </summary>
        /// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> instance.</param>
        /// <param name="requestPolicy">The policy or wrapped policies to apply to requests.</param>
        void Configure(IHttpClientFactory httpClientFactory, IAsyncPolicy<HttpResponseMessage> requestPolicy);
    }
}
