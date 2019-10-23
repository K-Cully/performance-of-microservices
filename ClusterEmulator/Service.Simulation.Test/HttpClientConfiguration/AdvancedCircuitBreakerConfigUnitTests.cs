using ClusterEmulator.Service.Simulation.HttpClientConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using System;
using System.Net.Http;

namespace ClusterEmulator.Service.Simulation.Test.HttpClientConfiguration
{
    [TestClass]
    public class AdvancedCircuitBreakerConfigUnitTests
    {
        [TestMethod]
        public void Deserialization_ValidData_CreatesValidInstance()
        {
            AdvancedCircuitBreakerConfig config = JsonConvert.DeserializeObject<AdvancedCircuitBreakerConfig>(
                "{ breakDuration : 12.5, threshold : 0.5, samplingDuration : 5.1, throughput : 22 }");

            Assert.IsNotNull(config);
            Assert.AreEqual(12.5d, config.BreakDuration, 0.0001d);
            Assert.AreEqual(0.5d, config.FailureThreshold, 0.0001d);
            Assert.AreEqual(5.1d, config.SamplingDuration, 0.0001d);
            Assert.AreEqual(22, config.MinimumThroughput);
        }


        [TestMethod]
        public void Deserialization_InvalidData_Throws()
        {
            Assert.ThrowsException<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<AdvancedCircuitBreakerConfig>("{ }"));
        }


        [TestMethod]
        public void AsPolicy_LoggerNull_Throws()
        {
            var config = new AdvancedCircuitBreakerConfig
            {
                BreakDuration = 1.0d,
                FailureThreshold = 0.75d,
                MinimumThroughput = 15,
                SamplingDuration = 3.2d
            };

            Assert.ThrowsException<ArgumentNullException>(
                () => config.AsTypeModel(null));
        }


        [TestMethod]
        public void AsPolicy_InvalidBreakDuration_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new AdvancedCircuitBreakerConfig
            {
                BreakDuration = 0.01d,
                FailureThreshold = 0.75d,
                MinimumThroughput = 15,
                SamplingDuration = 3.2d
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => config.AsTypeModel(logger.Object));
        }


        [TestMethod]
        public void AsPolicy_InvalidFailureThreshold_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new AdvancedCircuitBreakerConfig
            {
                BreakDuration = 1.0d,
                FailureThreshold = 1.2d,
                MinimumThroughput = 15,
                SamplingDuration = 3.2d
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => config.AsTypeModel(logger.Object));
        }


        [TestMethod]
        public void AsPolicy_InvalidMinimumthroughput_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new AdvancedCircuitBreakerConfig
            {
                BreakDuration = 1.0d,
                FailureThreshold = 0.3d,
                MinimumThroughput = -1,
                SamplingDuration = 3.2d
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => config.AsTypeModel(logger.Object));
        }


        [TestMethod]
        public void AsPolicy_InvalidSamplingDuration_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new AdvancedCircuitBreakerConfig
            {
                BreakDuration = 1.0d,
                FailureThreshold = 0.6d,
                MinimumThroughput = 15,
                SamplingDuration = 0.01d
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => config.AsTypeModel(logger.Object));
        }


        [TestMethod]
        public void AsPolicy_AllValuesInitialized_ReturnsPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new AdvancedCircuitBreakerConfig
            {
                BreakDuration = 1.0d,
                FailureThreshold = 0.5d,
                MinimumThroughput = 15,
                SamplingDuration = 3.2d
            };

            IAsyncPolicy<HttpResponseMessage> policy = config.AsTypeModel(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(AsyncCircuitBreakerPolicy<HttpResponseMessage>));
        }
    }
}
