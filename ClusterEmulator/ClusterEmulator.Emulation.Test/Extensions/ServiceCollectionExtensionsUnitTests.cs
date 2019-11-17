using ClusterEmulator.Service.Simulation.Core;
using ClusterEmulator.Service.Simulation.Extensions;
using ClusterEmulator.Service.Simulation.HttpClientConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ClusterEmulator.Service.Simulation.Test.Processors
{
    [TestClass]
    public class ServiceCollectionExtensionsUnitTests
    {
        [TestMethod]
        public void AddSimulationEngineClients_NullCollection_Throws()
        {
            // Arrange
            var registry = new Mock<IRegistry>(MockBehavior.Strict);

            // Act & Verify
            Assert.ThrowsException<ArgumentNullException>(
                () => ServiceCollectionExtensions.AddSimulationEngineClients(null, registry.Object));
        }


        [TestMethod]
        public void AddSimulationEngineClients_NullRegistry_Throws()
        {
            // Arrange
            var serviceCollection = new Mock<IServiceCollection>(MockBehavior.Strict);

            // Act & Verify
            Assert.ThrowsException<ArgumentNullException>(
                () => ServiceCollectionExtensions.AddSimulationEngineClients(serviceCollection.Object, null));
        }


        [TestMethod]
        public void AddSimulationEngineClients_NullRegistryPolicies_Throws()
        {
            // Arrange
            var services = new ServiceCollection();
            var registry = new Mock<IRegistry>(MockBehavior.Strict);

            registry.Setup(r => r.Clients).Returns(new List<KeyValuePair<string, ClientConfig>>());
            registry.Setup(r => r.PolicyRegistry).Returns<IPolicyRegistry<string>>(null);

            // Act & Verify
            Assert.ThrowsException<InvalidOperationException>(
                () => services.AddSimulationEngineClients(registry.Object));
        }


        [TestMethod]
        public void AddSimulationEngineClients_NullRegistryClients_Throws()
        {
            // Arrange
            var services = new ServiceCollection();
            var registry = new Mock<IRegistry>(MockBehavior.Strict);

            registry.Setup(r => r.Clients).Returns<List<KeyValuePair<string, ClientConfig>>>(null);
            registry.Setup(r => r.PolicyRegistry).Returns(new PolicyRegistry());

            // Act & Verify
            Assert.ThrowsException<InvalidOperationException>(
                () => services.AddSimulationEngineClients(registry.Object));
        }


        [TestMethod]
        public void AddSimulationEngineClients_EmptyLists_ReturnsCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var registry = new Mock<IRegistry>(MockBehavior.Strict);

            registry.Setup(r => r.Clients).Returns(new List<KeyValuePair<string, ClientConfig>>());
            registry.Setup(r => r.PolicyRegistry).Returns(new PolicyRegistry());

            // Act
            services.AddSimulationEngineClients(registry.Object);
            ServiceProvider provider = services.BuildServiceProvider();

            // Verify
            Assert.AreEqual(2, services.Count);
            Assert.IsNotNull(provider.GetRequiredService<IPolicyRegistry<string>>());
            Assert.IsNotNull(provider.GetRequiredService<IReadOnlyPolicyRegistry<string>>());
        }


        [TestMethod]
        public void AddSimulationEngineClients_CompleteLists_ReturnsCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var registry = new Mock<IRegistry>(MockBehavior.Strict);

            // Create client configs
            registry.Setup(r => r.Clients)
                .Returns(new List<KeyValuePair<string, ClientConfig>>
                    {
                        ConfigWithPolicies("config1", "policy1", "policy2"),
                        ConfigWithPolicies("config2", "policy2", "policy3"),
                    });

            // Create policy mocks
            var policyRegistry = new Mock<IPolicyRegistry<string>>(MockBehavior.Strict);
            policyRegistry.Setup(pr => pr.Get<IAsyncPolicy<HttpResponseMessage>>(It.IsAny<string>()))
                .Returns(Policy.NoOpAsync<HttpResponseMessage>());
            registry.Setup(r => r.PolicyRegistry).Returns(policyRegistry.Object);

            // Act
            services.AddSimulationEngineClients(registry.Object);
            ServiceProvider provider = services.BuildServiceProvider();
            IHttpClientFactory clientFactory = provider.GetRequiredService<IHttpClientFactory>();

            // Verify
            Assert.AreEqual(24, services.Count);
            Assert.IsNotNull(clientFactory);
            Assert.IsNotNull(clientFactory.CreateClient("config1"));
            Assert.IsNotNull(clientFactory.CreateClient("config2"));
            Assert.IsNotNull(provider.GetRequiredService<IPolicyRegistry<string>>());
            Assert.IsNotNull(provider.GetRequiredService<IReadOnlyPolicyRegistry<string>>());
        }


        [TestMethod]
        public void AddSimulationEngineClients_MissingPolicies_Throws()
        {
            // Arrange
            var services = new ServiceCollection();
            var registry = new Mock<IRegistry>(MockBehavior.Strict);

            // Create client configs
            registry.Setup(r => r.Clients)
                .Returns(new List<KeyValuePair<string, ClientConfig>>
                    {
                        ConfigWithPolicies("config1", "policy1", "policy2"),
                        ConfigWithPolicies("config2", "policy2", "policy3"),
                    });

            // Create policy mocks
            var policyRegistry = new Mock<IPolicyRegistry<string>>(MockBehavior.Strict);
            policyRegistry.Setup(pr => pr.Get<IAsyncPolicy<HttpResponseMessage>>(It.IsAny<string>()))
                .Returns<IAsyncPolicy<HttpResponseMessage>>(null);
            registry.Setup(r => r.PolicyRegistry).Returns(policyRegistry.Object);

            // Act
            services.AddSimulationEngineClients(registry.Object);
            ServiceProvider provider = services.BuildServiceProvider();
            IHttpClientFactory clientFactory = provider.GetRequiredService<IHttpClientFactory>();

            // Verify
            Assert.AreEqual(24, services.Count);
            Assert.IsNotNull(clientFactory);
            Assert.ThrowsException<ArgumentNullException>(
                () => clientFactory.CreateClient("config1"));
        }


        private KeyValuePair<string, ClientConfig> ConfigWithPolicies(string name, params string[] args)
        {
            var config = new ClientConfig()
            {
                BaseAddress = "http://test.com",
                RequestHeaders = new Dictionary<string, string>() { { "name", "value" } },
                Policies = new List<string>(args ?? Array.Empty<string>())
            };

            return new KeyValuePair<string, ClientConfig>(name, config);
        }
    }
}
