using ClusterEmulator.Emulation.Core;
using ClusterEmulator.Emulation.HttpClientConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace ClusterEmulator.Emulation.Test.Core
{
    [TestClass]
    public class SimpleHttpClientFactoryUnitTests
    {
        [TestMethod]
        public void Constructor_NullClientConfigs_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new SimpleHttpClientFactory(null, logger.Object));
        }


        [TestMethod]
        public void Constructor_NullLogger_Throws()
        {
            var configs = new Dictionary<string, ClientConfig>();
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new SimpleHttpClientFactory(configs, null));
        }


        [TestMethod]
        public void Constructor_EmptyClientConfigs_Succeeds()
        {
            var configs = new Dictionary<string, ClientConfig>();
            var logger = new Mock<ILogger>(MockBehavior.Loose);

            var factory = new SimpleHttpClientFactory(configs, logger.Object);

            Assert.IsNotNull(factory);
        }


        [TestMethod]
        public void CreateClient_EmptyName_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var configs = new Dictionary<string, ClientConfig>();
            var factory = new SimpleHttpClientFactory(configs, logger.Object);

            Assert.ThrowsException<ArgumentException>(
                () => factory.CreateClient(string.Empty));
        }


        [TestMethod]
        public void CreateClient_NameNotRegistered_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var configs = new Dictionary<string, ClientConfig>();
            var factory = new SimpleHttpClientFactory(configs, logger.Object);

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
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var factory = new SimpleHttpClientFactory(configs, logger.Object);

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
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var factory = new SimpleHttpClientFactory(configs, logger.Object);

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
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var factory = new SimpleHttpClientFactory(configs, logger.Object);

            bool platformException = false;
            try
            {
                factory.CreateClient("Bob");
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e) when (e is ArgumentException || e is UriFormatException)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                platformException = true;
            }

            // When running dotnet test on Windows and Linux, different exceptions are encountered
            Assert.IsTrue(platformException, "The platform specific exception was encountered");
        }


        [TestMethod]
        public void CreateClient_UriAbsolute_HeadersNull_Succeeds()
        {
            var configs = new Dictionary<string, ClientConfig>()
            {
                { "Bob", new ClientConfig { BaseAddress = "http://test.com/", Policies = null, RequestHeaders = null } }
            };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var factory = new SimpleHttpClientFactory(configs, logger.Object);

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
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var factory = new SimpleHttpClientFactory(configs, logger.Object);

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
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var factory = new SimpleHttpClientFactory(configs, logger.Object);

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
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var factory = new SimpleHttpClientFactory(configs, logger.Object);

            HttpClient client = factory.CreateClient("Bob");

            Assert.IsNotNull(client);
            Assert.AreEqual("http://test.com/", client.BaseAddress.ToString());
            Assert.AreEqual(2, client.DefaultRequestHeaders.Count());

            client.Dispose();
        }
    }
}
