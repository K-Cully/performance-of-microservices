using CoreService.Simulation.HttpClientConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using System;
using System.Net.Http;

namespace CoreService.Test.Simulation.HttpClientConfiguration
{
    [TestClass]
    public class CircuitBreakerConfigUnitTests
    {
        [TestMethod]
        public void Deserialization_ValidData_CreatesValidInstance()
        {
            CircuitBreakerConfig config = JsonConvert.DeserializeObject<CircuitBreakerConfig>(
                "{ duration : 12.5, tolerance : 15 }");

            Assert.IsNotNull(config);
            Assert.AreEqual(12.5d, config.BreakDuration, 0.0001d);
            Assert.AreEqual(15, config.FaultTolerance);
        }


        [TestMethod]
        public void Deserialization_InvalidData_Throws()
        {
            Assert.ThrowsException<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<CircuitBreakerConfig>("{ }"));
        }

        [TestMethod]
        public void AsPolicy_LoggerNull_Throws()
        {
            var config = new CircuitBreakerConfig
            {
                BreakDuration = 1.0d,
                FaultTolerance = 25
            };

            Assert.ThrowsException<ArgumentNullException>(
                () => config.AsPolicy(null));
        }


        [TestMethod]
        public void AsPolicy_InvalidBreakDuration_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new CircuitBreakerConfig
            {
                BreakDuration = 0.01d,
                FaultTolerance = 25
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => config.AsPolicy(logger.Object));
        }


        [TestMethod]
        public void AsPolicy_InvalidFaultTolerance_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new CircuitBreakerConfig
            {
                BreakDuration = 15.0d,
                FaultTolerance = 0
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => config.AsPolicy(logger.Object));
        }


        [TestMethod]
        public void AsPolicy_AllValuesInitialized_ReturnsPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new CircuitBreakerConfig
            {
                BreakDuration = 15.0d,
                FaultTolerance = 25
            };

            IAsyncPolicy<HttpResponseMessage> policy = config.AsPolicy(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(AsyncCircuitBreakerPolicy<HttpResponseMessage>));
        }
    }
}
