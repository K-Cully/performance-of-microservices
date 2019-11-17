using ClusterEmulator.Emulation.HttpClientConfiguration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using System.Net.Http;

namespace ClusterEmulator.Emulation.Test.HttpClientConfiguration
{
    [TestClass]
    public class PolicyExtensionsUnitTests
    {
        [TestMethod]
        public void HandleHttpRequests_ReturnsPolicyBuilder()
        {
            var builder = PolicyExtensions.HandleHttpRequests();

            Assert.IsNotNull(builder);
            Assert.IsInstanceOfType(builder, typeof(PolicyBuilder<HttpResponseMessage>));
        }
    }
}
