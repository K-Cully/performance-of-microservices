using CoreService.Simulation.HttpClientConfiguration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Polly;
using Polly.Timeout;
using System;

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
        public void AsPolicy_NegativeTimeoutInSeconds_Throws()
        {
            var retryConfig = new TimeoutConfig
            {
                TimeoutInSeconds = -1.0d,
                CancelDelegates = true
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => retryConfig.AsPolicy());
        }


        [TestMethod]
        public void AsPolicy_NaNTimeoutInSeconds_Throws()
        {
            var retryConfig = new TimeoutConfig
            {
                TimeoutInSeconds = double.NaN,
                CancelDelegates = false
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => retryConfig.AsPolicy());
        }


        [TestMethod]
        public void AsPolicy_TimeoutInSecondsMaxValue_Throws()
        {
            var retryConfig = new TimeoutConfig
            {
                TimeoutInSeconds = double.MaxValue,
                CancelDelegates = false
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => retryConfig.AsPolicy());
        }


        [TestMethod]
        public void AsPolicy_TimeoutInSecondsValid_CancelDelegates_ReturnsPolicy()
        {
            var retryConfig = new TimeoutConfig
            {
                TimeoutInSeconds = 5.0d,
                CancelDelegates = true
            };

            IAsyncPolicy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(TimeoutPolicy));
        }


        [TestMethod]
        public void AsPolicy_TimeoutInSecondsValid_ReturnsPolicy()
        {
            var retryConfig = new TimeoutConfig
            {
                TimeoutInSeconds = 0.001d,
                CancelDelegates = false
            };

            IAsyncPolicy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(TimeoutPolicy));
        }
    }
}
