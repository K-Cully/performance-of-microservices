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
        /// Initializes a new instance of <see cref="SimpleHttpClientFactory"/>.
        /// </summary>
        /// <param name="clientConfigs">The client configurations to create clients from.</param>
        public SimpleHttpClientFactory(IDictionary<string, ClientConfig> clientConfigs)
        {
            clients = clientConfigs ?? throw new ArgumentNullException(nameof(clientConfigs));
        }


        /// <summary>
        /// Returns a new <see cref="HttpClient"/> instance.
        /// The caller must ensure that the client is disposed after use.
        /// </summary>
        /// <param name="name">The name of the client.</param>
        /// <returns>A new <see cref="HttpClient"/> instance.</returns>
        public HttpClient CreateClient(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"{nameof(name)} must be initialized", nameof(name));
            }

            if (!clients.ContainsKey(name))
            {
                throw new ArgumentException($"'{name}' is not registered", nameof(name));
            }

            var client = new HttpClient();
            try
            {
                var config = clients[name];
                client.BaseAddress = new Uri(config.BaseAddress, UriKind.Absolute);
                if (config.RequestHeaders != null)
                {
                    foreach ((string key, string value) in config.RequestHeaders)
                    {
                        client.DefaultRequestHeaders.Add(key, value);
                    }
                }
            }
            catch
            {
                // Ensure client is disposed if an exception occurrs
                client.Dispose();
                throw;
            }

            return client;
        }


        private readonly IDictionary<string, ClientConfig> clients;
    }
}
