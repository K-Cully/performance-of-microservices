using CoreService.Simulation.HttpClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using Polly.Timeout;
using System;

namespace CoreService.Test.Simulation.HttpClient
{
    [TestClass]
    public class TimeoutConfigUnitTests
    {
        [TestMethod]
        public void AsPolicy_NegativeTimeoutInSeconds_Throws()
        {
            var retryConfig = new TimeoutConfig
            {
                Async = false,
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
                Async = true,
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
                Async = true,
                TimeoutInSeconds = double.MaxValue,
                CancelDelegates = false
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => retryConfig.AsPolicy());
        }


        [TestMethod]
        public void AsPolicy_TimeoutInSecondsValid_CancelDelegates_Async_ReturnsPolicy()
        {
            var retryConfig = new TimeoutConfig
            {
                Async = true,
                TimeoutInSeconds = 1.0d,
                CancelDelegates = true
            };

            Policy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(TimeoutPolicy));
        }


        [TestMethod]
        public void AsPolicy_TimeoutInSecondsValid_CancelDelegates_ReturnsPolicy()
        {
            var retryConfig = new TimeoutConfig
            {
                Async = false,
                TimeoutInSeconds = 5.0d,
                CancelDelegates = true
            };

            Policy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(TimeoutPolicy));
        }


        [TestMethod]
        public void AsPolicy_TimeoutInSecondsValid_Async_ReturnsPolicy()
        {
            var retryConfig = new TimeoutConfig
            {
                Async = true,
                TimeoutInSeconds = 1000.0d,
                CancelDelegates = false
            };

            Policy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(TimeoutPolicy));
        }


        [TestMethod]
        public void AsPolicy_TimeoutInSecondsValid_ReturnsPolicy()
        {
            var retryConfig = new TimeoutConfig
            {
                Async = false,
                TimeoutInSeconds = 0.001d,
                CancelDelegates = false
            };

            Policy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(TimeoutPolicy));
        }
    }
}
