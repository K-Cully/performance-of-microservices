using ClusterEmulator.Emulation.HttpClientConfiguration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ClusterEmulator.Emulation.Core
{
    /// <summary>
    /// A simple factory for creating clients from client configurations.
    /// </summary>
    public class SimpleHttpClientFactory : IHttpClientFactory
    {
        private readonly IDictionary<string, ClientConfig> clients;
        private readonly ILogger log;


        /// <summary>
        /// Initializes a new instance of <see cref="SimpleHttpClientFactory"/>.
        /// </summary>
        /// <param name="clientConfigs">The client configurations to create clients from.</param>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        public SimpleHttpClientFactory(IDictionary<string, ClientConfig> clientConfigs, ILogger logger)
        {
            clients = clientConfigs ?? throw new ArgumentNullException(nameof(clientConfigs));
            log = logger ?? throw new ArgumentNullException(nameof(logger));
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
                log.LogWarning("{Client} is not registered", name);
                throw new ArgumentException($"'{name}' is not registered", nameof(name));
            }

            var client = new HttpClient();
            try
            {
                log.LogDebug("Creating {Client}", name);
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
            catch (Exception e)
            {
                // Ensure client is disposed if an exception occurrs
                log.LogError(e, "Error creating {Client}, disposing client and rethrowing", name);
                client.Dispose();
                throw;
            }

            log.LogInformation("{Client} created successfully", name);
            return client;
        }
    }
}
