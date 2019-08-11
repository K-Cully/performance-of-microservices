using CoreService.Simulation.HttpClientConfiguration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreService.Test.Simulation.HttpClientConfiguration
{
    [TestClass]
    public class RetryConfigUnitTests
    {
        [TestMethod]
        public void Deserialization_ValidData_CreatesValidInstance()
        {
            RetryConfig config = JsonConvert.DeserializeObject<RetryConfig>(
                "{ retries : 2, delays : [ 1, 2 ], jitter : 300 }");

            Assert.IsNotNull(config);
            Assert.AreEqual(2, config.Retries);
            Assert.AreEqual(2, config.DelaysInSeconds.Count());
            Assert.AreEqual(1.0d, config.DelaysInSeconds.First(), 0.0001d);
            Assert.AreEqual(300, config.JitterMilliseconds);
        }


        [TestMethod]
        public void Deserialization_InvalidData_Throws()
        {
            Assert.ThrowsException<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<RetryConfig>("{ }"));
        }


        [TestMethod]
        public void AsPolicy_DelaysInSecondsNull_Throws()
        {
            var retryConfig = new RetryConfig
            {
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
                DelaysInSeconds = new List<double>(),
                JitterMilliseconds = 0,
                Retries = -1
            };

            IAsyncPolicy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_DelaysEmpty_RetriesZero_ReturnsForeverRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double>(),
                JitterMilliseconds = 222,
                Retries = 0
            };

            IAsyncPolicy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_DelaysEmpty_RetriesPositive_ReturnsRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double>(),
                JitterMilliseconds = 0,
                Retries = 1
            };

            IAsyncPolicy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_NegativeDelay_OtherThanMinusOne_Throws()
        {
            var retryConfig = new RetryConfig
            {
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
                DelaysInSeconds = new List<double> { -1.0d },
                JitterMilliseconds = 15,
                Retries = 1
            };

            IAsyncPolicy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_MinusOneDelay_ZeroRetries_ReturnsExponentialForeverRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double> { -1.0d },
                JitterMilliseconds = 15,
                Retries = 0
            };

            IAsyncPolicy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_MinusOneDelay_NegativeRetries_ReturnsExponentialForeverRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double> { -1.0d },
                JitterMilliseconds = 0,
                Retries = -100
            };

            IAsyncPolicy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_RetriesEqualToDelays_ReturnsRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double> { 1.0d },
                JitterMilliseconds = 0,
                Retries = 1
            };

            IAsyncPolicy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_RetriesShorterThanDelays_ReturnsRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double> { 1.0d, 2.0d },
                JitterMilliseconds = 0,
                Retries = 1
            };

            IAsyncPolicy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_RetriesLongerThanDelays_ReturnsRetryPolicy()
        {
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double> { 2.0d },
                JitterMilliseconds = 0,
                Retries = 5
            };

            IAsyncPolicy policy = retryConfig.AsPolicy();

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }
    }
}
