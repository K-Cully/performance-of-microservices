using ClusterEmulator.Emulation.HttpClientConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Polly;
using Polly.Fallback;
using System;
using System.Net.Http;

namespace ClusterEmulator.Emulation.Test.HttpClientConfiguration
{
    [TestClass]
    public class FallbackConfigUnitTests
    {
        [TestMethod]
        public void Deserialization_AllData_CreatesValidInstance()
        {
            FallbackConfig config = JsonConvert.DeserializeObject<FallbackConfig>(
                "{ statusCode : 200, reason : 'things', content : 'who knows?' }");

            Assert.IsNotNull(config);
            Assert.AreEqual(200, config.Status);
            Assert.AreEqual("things", config.Reason);
            Assert.AreEqual("who knows?", config.Content);
        }


        [TestMethod]
        public void Deserialization_ValidData_CreatesValidInstance()
        {
            FallbackConfig config = JsonConvert.DeserializeObject<FallbackConfig>(
                "{ statusCode : 8999 }");

            Assert.IsNotNull(config);
            Assert.AreEqual(8999, config.Status);
            Assert.AreEqual(null, config.Reason);
            Assert.AreEqual(null, config.Content);
        }


        [TestMethod]
        public void Deserialization_InvalidData_Throws()
        {
            Assert.ThrowsException<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<FallbackConfig>("{ }"));
        }

        [TestMethod]
        public void AsPolicy_LoggerNull_Throws()
        {
            var config = new FallbackConfig
            {
                Status = 418,
                Content = string.Empty,
                Reason = string.Empty
            };

            Assert.ThrowsException<ArgumentNullException>(
                () => config.AsTypeModel(null));
        }


        [TestMethod]
        public void AsPolicy_InvalidStatusCode_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new FallbackConfig
            {
                Status = -1
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => config.AsTypeModel(logger.Object));
        }


        [TestMethod]
        public void AsPolicy_RequiredValuesInitialized_ReturnsPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new FallbackConfig
            {
                Status = 418,
                Content = null,
                Reason = null
            };

            IAsyncPolicy<HttpResponseMessage> policy = config.AsTypeModel(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(AsyncFallbackPolicy<HttpResponseMessage>));
        }


        [TestMethod]
        public void AsPolicy_AllValuesInitialized_ReturnsPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new FallbackConfig
            {
                Status = 418,
                Content = "test",
                Reason = "test"
            };

            IAsyncPolicy<HttpResponseMessage> policy = config.AsTypeModel(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(AsyncFallbackPolicy<HttpResponseMessage>));
        }
    }
}
