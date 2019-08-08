using CoreService.Simulation.HttpClientConfiguration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;

namespace CoreService.Test.Simulation.HttpClient
{
    [TestClass]
    public class RetryConfigUnitTests
    {
        [TestMethod]
        public void AsPolicy_DelaysInSecondsNull_Throws()
        {
            var retryConfig = new RetryConfig
            {
                Async = false,
                DelaysInSeconds = null,
                JitterMilliseconds = 10,
                Retries = 3
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => retryConfig.AsPolicy());
        }


        [TestMethod]
        public void AsPolicy_NegativeJitter_Throws()
        {
            var retryConfig = new RetryConfig
            {
                Async = false,
                DelaysInSeconds = new List<double>(),
                JitterMilliseconds = -10,
                Retries = 3
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => retryConfig.AsPolicy());
        }


        [TestMethod]
        public void AsPolicy_DelaysEmpty_RetriesNegative_ReturnsForeverRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                Async = false,
                DelaysInSeconds = new List<double>(),
                JitterMilliseconds = 0,
                Retries = -1
            };

            Policy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_DelaysEmpty_RetriesZero_Async_ReturnsForeverRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                Async = true,
                DelaysInSeconds = new List<double>(),
                JitterMilliseconds = 222,
                Retries = 0
            };

            Policy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_DelaysEmpty_RetriesPositive_ReturnsRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                Async = false,
                DelaysInSeconds = new List<double>(),
                JitterMilliseconds = 0,
                Retries = 1
            };

            Policy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_DelaysEmpty_RetriesPositive_Async_ReturnsRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                Async = true,
                DelaysInSeconds = new List<double>(),
                JitterMilliseconds = 12,
                Retries = 12
            };

            Policy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_NegativeDelay_OtherThanMinusOne_Throws()
        {
            var retryConfig = new RetryConfig
            {
                Async = false,
                DelaysInSeconds = new List<double> { -0.1d },
                JitterMilliseconds = 0,
                Retries = 1
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => retryConfig.AsPolicy());
        }


        [TestMethod]
        public void AsPolicy_TwoNegativeDelays_WithMinusOneFirst_Throws()
        {
            var retryConfig = new RetryConfig
            {
                Async = false,
                DelaysInSeconds = new List<double> { -1.0d, -0.1d },
                JitterMilliseconds = 0,
                Retries = 1
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => retryConfig.AsPolicy());
        }


        [TestMethod]
        public void AsPolicy_MinusOneDelay_ReturnsExponentialRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                Async = false,
                DelaysInSeconds = new List<double> { -1.0d },
                JitterMilliseconds = 15,
                Retries = 1
            };

            Policy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_MinusOneDelay_Async_ReturnsExponentialRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                Async = true,
                DelaysInSeconds = new List<double> { -1.0d },
                JitterMilliseconds = 0,
                Retries = 5
            };

            Policy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_MinusOneDelay_ZeroRetries_ReturnsExponentialForeverRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                Async = false,
                DelaysInSeconds = new List<double> { -1.0d },
                JitterMilliseconds = 15,
                Retries = 0
            };

            Policy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_MinusOneDelay_NegativeRetries_Async_ReturnsExponentialForeverRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                Async = true,
                DelaysInSeconds = new List<double> { -1.0d },
                JitterMilliseconds = 0,
                Retries = -100
            };

            Policy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_RetriesEqualToDelays_ReturnsRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                Async = false,
                DelaysInSeconds = new List<double> { 1.0d },
                JitterMilliseconds = 0,
                Retries = 1
            };

            Policy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_RetriesEqualToDelays_Async_ReturnsRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                Async = false,
                DelaysInSeconds = new List<double> { 1.0d },
                JitterMilliseconds = 0,
                Retries = 1
            };

            Policy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_RetriesShorterThanDelays_ReturnsRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                Async = false,
                DelaysInSeconds = new List<double> { 1.0d, 2.0d },
                JitterMilliseconds = 0,
                Retries = 1
            };

            Policy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_RetriesShorterThanDelays_Async_ReturnsRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                Async = false,
                DelaysInSeconds = new List<double> { 1.0d, 2.0d },
                JitterMilliseconds = 0,
                Retries = 1
            };

            Policy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_RetriesLongerThanDelays_ReturnsRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                Async = false,
                DelaysInSeconds = new List<double> { 2.0d },
                JitterMilliseconds = 0,
                Retries = 5
            };

            Policy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_RetriesLongerThanDelays_Async_ReturnsRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                Async = false,
                DelaysInSeconds = new List<double> { 2.0d },
                JitterMilliseconds = 0,
                Retries = 100
            };

            Policy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }
    }
}
