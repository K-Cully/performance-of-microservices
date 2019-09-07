using CoreService.Simulation.HttpClientConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

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
        public void AsPolicy_LoggerNull_Throws()
        {
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double>(),
                JitterMilliseconds = 10,
                Retries = 3
            };

            Assert.ThrowsException<ArgumentNullException>(
                () => retryConfig.AsPolicy(null));
        }


        [TestMethod]
        public void AsPolicy_DelaysInSecondsNull_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = null,
                JitterMilliseconds = 10,
                Retries = 3
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => retryConfig.AsPolicy(logger.Object));
        }


        [TestMethod]
        public void AsPolicy_NegativeJitter_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double>(),
                JitterMilliseconds = -10,
                Retries = 3
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => retryConfig.AsPolicy(logger.Object));
        }


        [TestMethod]
        public void AsPolicy_DelaysEmpty_RetriesNegative_ReturnsForeverRetryPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double>(),
                JitterMilliseconds = 0,
                Retries = -1
            };

            IAsyncPolicy<HttpResponseMessage> policy = retryConfig.AsPolicy(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_DelaysEmpty_RetriesZero_ReturnsForeverRetryPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double>(),
                JitterMilliseconds = 222,
                Retries = 0
            };

            IAsyncPolicy<HttpResponseMessage> policy = retryConfig.AsPolicy(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_DelaysEmpty_RetriesPositive_ReturnsRetryPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double>(),
                JitterMilliseconds = 0,
                Retries = 1
            };

            IAsyncPolicy<HttpResponseMessage> policy = retryConfig.AsPolicy(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_NegativeDelay_OtherThanMinusOne_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double> { -0.1d },
                JitterMilliseconds = 0,
                Retries = 1
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => retryConfig.AsPolicy(logger.Object));
        }


        [TestMethod]
        public void AsPolicy_TwoNegativeDelays_WithMinusOneFirst_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double> { -1.0d, -0.1d },
                JitterMilliseconds = 0,
                Retries = 1
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => retryConfig.AsPolicy(logger.Object));
        }


        [TestMethod]
        public void AsPolicy_MinusOneDelay_ReturnsExponentialRetryPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double> { -1.0d },
                JitterMilliseconds = 15,
                Retries = 1
            };

            IAsyncPolicy<HttpResponseMessage> policy = retryConfig.AsPolicy(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_MinusOneDelay_ZeroRetries_ReturnsExponentialForeverRetryPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double> { -1.0d },
                JitterMilliseconds = 15,
                Retries = 0
            };

            IAsyncPolicy<HttpResponseMessage> policy = retryConfig.AsPolicy(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_MinusOneDelay_NegativeRetries_ReturnsExponentialForeverRetryPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double> { -1.0d },
                JitterMilliseconds = 0,
                Retries = -100
            };

            IAsyncPolicy<HttpResponseMessage> policy = retryConfig.AsPolicy(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_RetriesEqualToDelays_ReturnsRetryPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double> { 1.0d },
                JitterMilliseconds = 0,
                Retries = 1
            };

            IAsyncPolicy<HttpResponseMessage> policy = retryConfig.AsPolicy(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_RetriesShorterThanDelays_ReturnsRetryPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double> { 1.0d, 2.0d },
                JitterMilliseconds = 0,
                Retries = 1
            };

            IAsyncPolicy<HttpResponseMessage> policy = retryConfig.AsPolicy(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }


        [TestMethod]
        public void AsPolicy_RetriesLongerThanDelays_ReturnsRetryPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var retryConfig = new RetryConfig
            {
                DelaysInSeconds = new List<double> { 2.0d },
                JitterMilliseconds = 0,
                Retries = 5
            };

            IAsyncPolicy<HttpResponseMessage> policy = retryConfig.AsPolicy(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy));
        }
    }
}
