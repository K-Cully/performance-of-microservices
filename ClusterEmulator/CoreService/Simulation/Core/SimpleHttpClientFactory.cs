using CoreService.Simulation.HttpClientConfiguration;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace CoreService.Simulation.Core
{
    /// <summary>
    /// A simple factory for creating clients from client configurations.
    /// </summary>
    public class SimpleHttpClientFactory : IHttpClientFactory
    {
        /// <summary>
        /// Returns a new <see cref="HttpClient"/> instance.
        /// The caller must ensure that the client is disposed after use.
        /// </summary>
        /// <param name="name">The name of the client.</param>
        /// <returns>A new <see cref="HttpClient"/> instance.</returns>
        public HttpClient CreateClient(string name)
        {
            // TODO: add validation to name

            var client = new HttpClient();
            var config = Clients[name];

            client.BaseAddress = new Uri(config.BaseAddress, UriKind.Absolute);
            foreach ((var key, var value) in config.RequestHeaders)
            {
                client.DefaultRequestHeaders.Add(key, value);
            }

            return client;
        }


        /// <summary>
        /// Gets or sets the client configurations.
        /// </summary>
        public IDictionary<string, ClientConfig> Clients { get; set; }
    }
}
