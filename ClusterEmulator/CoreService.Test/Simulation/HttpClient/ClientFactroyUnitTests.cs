using CoreService.Simulation.HttpClientConfiguration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreService.Test.Simulation.Processors
{
    [TestClass]
    public class ClientFactroyUnitTests
    {
        [TestMethod]
        public void Create_WithNullName_ThrowsException()
        {
            var factory = new ClientFactory();

            Assert.ThrowsException<ArgumentException>(
                () => factory.Create(null), "Create should throw.");
        }


        [TestMethod]
        public void Create_WithValidSetting_ReturnsClientConfig()
        {
            HashSet<string> policies = new HashSet<string>{ "F", "C", "A" };
            string setting = "{ baseAddress : 'https://github.com/', policies : [ 'C', 'A', 'F' ], headers : { 'Accept' : 'application/json' } }";
            var factory = new ClientFactory();

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
            var factory = new ClientFactory();

            ClientConfig client = factory.Create(setting);

            Assert.IsNull(client, "Client config should be null");
        }


        [TestMethod]
        public void Create_WithEmptyJson_ReturnsNull()
        {
            string setting = "{ }";
            var factory = new ClientFactory();

            ClientConfig client = factory.Create(setting);

            Assert.IsNull(client, "Client config should be null");
        }
    }
}
