using CoreService.Simulation.Core;
using CoreService.Simulation.HttpClientConfiguration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace CoreService.Test.Simulation.Core
{
    [TestClass]
    public class SimpleHttpClientFactoryUnitTests
    {
        [TestMethod]
        public void Constructor_NullClientConfigs_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new SimpleHttpClientFactory(null));
        }


        [TestMethod]
        public void Constructor_EmptyClientConfigs_Succeeds()
        {
            var configs = new Dictionary<string, ClientConfig>();

            var factory = new SimpleHttpClientFactory(configs);

            Assert.IsNotNull(factory);
        }


        [TestMethod]
        public void CreateClient_EmptyName_Throws()
        {
            var configs = new Dictionary<string, ClientConfig>();
            var factory = new SimpleHttpClientFactory(configs);

            Assert.ThrowsException<ArgumentException>(
                () => factory.CreateClient(string.Empty));
        }


        [TestMethod]
        public void CreateClient_NameNotRegistered_Throws()
        {
            var configs = new Dictionary<string, ClientConfig>();
            var factory = new SimpleHttpClientFactory(configs);

            Assert.ThrowsException<ArgumentException>(
                () => factory.CreateClient("Missing"));
        }


        [TestMethod]
        public void CreateClient_EmptyConfig_Throws()
        {
            var configs = new Dictionary<string, ClientConfig>()
            {
                { "Bob", new ClientConfig() }
            };
            var factory = new SimpleHttpClientFactory(configs);

            Assert.ThrowsException<ArgumentNullException>(
                () => factory.CreateClient("Bob"));
        }


        [TestMethod]
        public void CreateClient_UriEmpty_Throws()
        {
            var configs = new Dictionary<string, ClientConfig>()
            {
                { "Bob", new ClientConfig { BaseAddress = string.Empty, Policies = null, RequestHeaders = null } }
            };
            var factory = new SimpleHttpClientFactory(configs);

            Assert.ThrowsException<UriFormatException>(
                () => factory.CreateClient("Bob"));
        }


        [TestMethod]
        public void CreateClient_UriRelative_Throws()
        {
            var configs = new Dictionary<string, ClientConfig>()
            {
                { "Bob", new ClientConfig { BaseAddress = "/test/", Policies = null, RequestHeaders = null } }
            };
            var factory = new SimpleHttpClientFactory(configs);

            Assert.ThrowsException<UriFormatException>(
                () => factory.CreateClient("Bob"));
        }


        [TestMethod]
        public void CreateClient_UriAbsolute_HeadersNull_Succeeds()
        {
            var configs = new Dictionary<string, ClientConfig>()
            {
                { "Bob", new ClientConfig { BaseAddress = "http://test.com/", Policies = null, RequestHeaders = null } }
            };
            var factory = new SimpleHttpClientFactory(configs);

            HttpClient client = factory.CreateClient("Bob");

            Assert.IsNotNull(client);
            Assert.AreEqual("http://test.com/", client.BaseAddress.ToString());
            Assert.AreEqual(0, client.DefaultRequestHeaders.Count());

            client.Dispose();
        }


        [TestMethod]
        public void CreateClient_UriAbsolute_HeadersEmpty_Succeeds()
        {
            var headers = new Dictionary<string, string>();
            var configs = new Dictionary<string, ClientConfig>
            {
                { "Bob", new ClientConfig { BaseAddress = "http://test.com/", Policies = null, RequestHeaders = headers } }
            };
            var factory = new SimpleHttpClientFactory(configs);

            HttpClient client = factory.CreateClient("Bob");

            Assert.IsNotNull(client);
            Assert.AreEqual("http://test.com/", client.BaseAddress.ToString());
            Assert.AreEqual(0, client.DefaultRequestHeaders.Count());

            client.Dispose();
        }


        [TestMethod]
        public void CreateClient_UriAbsolute_NullHeaderValue_Succeeds()
        {
            var headers = new Dictionary<string, string>
            {
                { "header1", null }
            };

            var configs = new Dictionary<string, ClientConfig>
            {
                { "Bob", new ClientConfig { BaseAddress = "http://test.com/", Policies = null, RequestHeaders = headers } }
            };
            var factory = new SimpleHttpClientFactory(configs);

            HttpClient client = factory.CreateClient("Bob");

            Assert.IsNotNull(client);
            Assert.AreEqual("http://test.com/", client.BaseAddress.ToString());
            Assert.AreEqual(1, client.DefaultRequestHeaders.Count());

            client.Dispose();
        }


        [TestMethod]
        public void CreateClient_UriAbsolute_InitializedHeaderValues_Succeeds()
        {
            var headers = new Dictionary<string, string>
            {
                { "header1", "test1" },
                { "header2", "test2" }
            };

            var configs = new Dictionary<string, ClientConfig>
            {
                { "Bob", new ClientConfig { BaseAddress = "http://test.com/", Policies = null, RequestHeaders = headers } }
            };
            var factory = new SimpleHttpClientFactory(configs);

            HttpClient client = factory.CreateClient("Bob");

            Assert.IsNotNull(client);
            Assert.AreEqual("http://test.com/", client.BaseAddress.ToString());
            Assert.AreEqual(2, client.DefaultRequestHeaders.Count());

            client.Dispose();
        }
    }
}
