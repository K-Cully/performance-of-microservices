using CoreService.Simulation.HttpClientConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Polly;
using Polly.Timeout;
using System;
using System.Net.Http;

namespace CoreService.Test.Simulation.HttpClientConfiguration
{
    [TestClass]
    public class TimeoutConfigUnitTests
    {
        [TestMethod]
        public void Deserialization_ValidData_CreatesValidInstance()
        {
            TimeoutConfig config = JsonConvert.DeserializeObject<TimeoutConfig>(
                "{ time : 2.2, cancelDelegates : false }");

            Assert.IsNotNull(config);
            Assert.AreEqual(2.2d, config.TimeoutInSeconds, 0.0001d);
            Assert.IsFalse(config.CancelDelegates);
        }


        [TestMethod]
        public void Deserialization_InvalidData_Throws()
        {
            Assert.ThrowsException<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<TimeoutConfig>("{ }"));
        }


        [TestMethod]
        public void AsPolicy_LoggerNull_Throws()
        {
            var config = new TimeoutConfig
            {
                TimeoutInSeconds = 1.0d,
                CancelDelegates = true
            };

            Assert.ThrowsException<ArgumentNullException>(
                () => config.AsPolicy(null));
        }


        [TestMethod]
        public void AsPolicy_NegativeTimeoutInSeconds_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new TimeoutConfig
            {
                TimeoutInSeconds = -1.0d,
                CancelDelegates = true
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => config.AsPolicy(logger.Object));
        }


        [TestMethod]
        public void AsPolicy_NaNTimeoutInSeconds_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new TimeoutConfig
            {
                TimeoutInSeconds = double.NaN,
                CancelDelegates = false
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => config.AsPolicy(logger.Object));
        }


        [TestMethod]
        public void AsPolicy_TimeoutInSecondsMaxValue_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new TimeoutConfig
            {
                TimeoutInSeconds = double.MaxValue,
                CancelDelegates = false
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => config.AsPolicy(logger.Object));
        }


        [TestMethod]
        public void AsPolicy_TimeoutInSecondsValid_CancelDelegates_ReturnsPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new TimeoutConfig
            {
                TimeoutInSeconds = 5.0d,
                CancelDelegates = true
            };

            IAsyncPolicy<HttpResponseMessage> policy = config.AsPolicy(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(TimeoutPolicy));
        }


        [TestMethod]
        public void AsPolicy_TimeoutInSecondsValid_ReturnsPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new TimeoutConfig
            {
                TimeoutInSeconds = 0.001d,
                CancelDelegates = false
            };

            IAsyncPolicy<HttpResponseMessage> policy = config.AsPolicy(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(TimeoutPolicy));
        }
    }
}
