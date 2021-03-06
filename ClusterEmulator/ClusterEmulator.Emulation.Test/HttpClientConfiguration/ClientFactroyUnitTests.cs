﻿using ClusterEmulator.Emulation.Core;
using ClusterEmulator.Emulation.HttpClientConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClusterEmulator.Emulation.Test.HttpClientConfiguration
{
    [TestClass]
    public class ClientFactroyUnitTests
    {
        [TestMethod]
        public void Constructor_WithNullLogger_ThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new ConfigFactory<ClientConfig>(null), "Constructor should throw.");
        }


        [TestMethod]
        public void Create_WithNullName_ThrowsException()
        {
            var logger = new Mock<ILogger<ConfigFactory<ClientConfig>>>(MockBehavior.Loose);
            var factory = new ConfigFactory<ClientConfig>(logger.Object);

            Assert.ThrowsException<ArgumentException>(
                () => factory.Create(null), "Create should throw.");
        }


        [TestMethod]
        public void Create_WithValidSetting_ReturnsClientConfig()
        {
            HashSet<string> policies = new HashSet<string>{ "F", "C", "A" };
            string setting = "{ baseAddress : 'https://github.com/', policies : [ 'C', 'A', 'F' ], headers : { 'Accept' : 'application/json' } }";
            var logger = new Mock<ILogger<ConfigFactory<ClientConfig>>>(MockBehavior.Loose);
            var factory = new ConfigFactory<ClientConfig>(logger.Object);

            ClientConfig client = factory.Create(setting);

            Assert.IsNotNull(client, "Client config should not be null");
            Assert.AreEqual("https://github.com/", client.BaseAddress, "Base address should be set correctly");
            Assert.IsTrue(policies.SetEquals(client.Policies), "Policies should be set correctly");
            Assert.AreEqual("C", client.Policies.First(), "Policies should be in correct order");
            Assert.IsTrue(client.RequestHeaders.ContainsKey("Accept"), "Request header should be present");
            Assert.AreEqual("application/json", client.RequestHeaders["Accept"], "Request header should be set correctly");
        }


        [TestMethod]
        public void Create_WithNonJsonData_ReturnsNull()
        {
            string setting = "???";
            var logger = new Mock<ILogger<ConfigFactory<ClientConfig>>>(MockBehavior.Loose);
            var factory = new ConfigFactory<ClientConfig>(logger.Object);

            ClientConfig client = factory.Create(setting);

            Assert.IsNull(client, "Client config should be null");
        }


        [TestMethod]
        public void Create_WithEmptyJson_ReturnsNull()
        {
            string setting = "{ }";
            var logger = new Mock<ILogger<ConfigFactory<ClientConfig>>>(MockBehavior.Loose);
            var factory = new ConfigFactory<ClientConfig>(logger.Object);

            ClientConfig client = factory.Create(setting);

            Assert.IsNull(client, "Client config should be null");
        }
    }
}
