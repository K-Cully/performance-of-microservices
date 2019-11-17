using ClusterEmulator.Emulation.HttpClientConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Polly;
using Polly.Caching;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace ClusterEmulator.Emulation.Test.HttpClientConfiguration
{
    [TestClass]
    public class CacheConfigUnitTests
    {
        [TestMethod]
        public void CacheTime_AsTimeSpan_ValidTime_ReturnsTimeSpan()
        {
            CacheTime time = new CacheTime { Minutes = 1 };

            Assert.AreEqual(TimeSpan.FromMinutes(1), time.AsTimeSpan());
        }


        [TestMethod]
        public void CacheTime_AsTimeSpan_AllZero_ReturnsZero()
        {
            CacheTime time = new CacheTime { Days = 0, Hours = 0, Minutes = 0, Seconds = 0 };

            Assert.AreEqual(TimeSpan.Zero, time.AsTimeSpan());
        }


        [TestMethod]
        public void CacheTime_AsTimeSpan_NegativeValue_ReturnsZero()
        {
            CacheTime time = new CacheTime { Days = 1, Hours = 2, Minutes = 3, Seconds = -1 };

            Assert.AreEqual(TimeSpan.Zero, time.AsTimeSpan());
        }


        [TestMethod]
        public void CacheTime_AsTimeSpan_MaxValue_ReturnsZero()
        {
            CacheTime time = new CacheTime { Days = int.MaxValue, Hours = 2, Minutes = 3, Seconds = 1 };

            Assert.AreEqual(TimeSpan.Zero, time.AsTimeSpan());
        }


        [TestMethod]
        public void Deserialization_AllData_CreatesValidInstance()
        {
            CacheConfig config = JsonConvert.DeserializeObject<CacheConfig>(
                "{ time: { days: 1, hours: 2, minutes: 3, seconds: 4 }, absoluteTime: true, slidingExpiration: true }");

            Assert.IsNotNull(config);
            Assert.IsNotNull(config.Time);
            Assert.AreEqual(1, config.Time.Days);
            Assert.AreEqual(2, config.Time.Hours);
            Assert.AreEqual(3, config.Time.Minutes);
            Assert.AreEqual(4, config.Time.Seconds);
            Assert.IsTrue(config.Absolute);
            Assert.IsTrue(config.Sliding);
        }


        [TestMethod]
        public void Deserialization_ValidData_CreatesValidInstance()
        {
            CacheConfig config = JsonConvert.DeserializeObject<CacheConfig>(
                "{ time : { seconds: 1} }");

            Assert.IsNotNull(config);
            Assert.IsNotNull(config.Time);
            Assert.AreEqual(1, config.Time.Seconds);
            Assert.AreEqual(0, config.Time.Days);
            Assert.AreEqual(0, config.Time.Hours);
            Assert.AreEqual(0, config.Time.Minutes);
            Assert.IsFalse(config.Absolute);
            Assert.IsFalse(config.Sliding);
        }


        [TestMethod]
        public void Deserialization_InvalidData_Throws()
        {
            Assert.ThrowsException<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<CacheConfig>("{ }"));
        }


        [TestMethod]
        public void AsPolicy_LoggerNull_Throws()
        {
            var provider = new Mock<IAsyncCacheProvider<HttpResponseMessage>>(MockBehavior.Strict);
            var config = new CacheConfig(provider.Object)
            {
                Time = new CacheTime { Days = 0, Hours = 0, Minutes = 5, Seconds = 0 },
                Absolute = false,
                Sliding = false
            };

            Assert.ThrowsException<ArgumentNullException>(
                () => config.AsTypeModel(null));
        }


        [TestMethod]
        public void AsPolicy_TimeNull_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var provider = new Mock<IAsyncCacheProvider<HttpResponseMessage>>(MockBehavior.Strict);
            var config = new CacheConfig(provider.Object)
            {
                Time = null,
                Absolute = false,
                Sliding = false
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => config.AsTypeModel(logger.Object));
        }


        [TestMethod]
        public void AsPolicy_Relative_ReturnsPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var provider = new Mock<IAsyncCacheProvider<HttpResponseMessage>>(MockBehavior.Strict);
            var config = new CacheConfig(provider.Object)
            {
                Time = new CacheTime { Days = 0, Hours = 0, Minutes = 5, Seconds = 0 },
                Absolute = false,
                Sliding = false
            };

            IAsyncPolicy<HttpResponseMessage> policy = config.AsTypeModel(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(AsyncCachePolicy<HttpResponseMessage>));
        }


        [TestMethod]
        public void AsPolicy_Sliding_ReturnsPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var provider = new Mock<IAsyncCacheProvider<HttpResponseMessage>>(MockBehavior.Strict);
            var config = new CacheConfig(provider.Object)
            {
                Time = new CacheTime { Days = 0, Hours = 1, Minutes = 5, Seconds = 0 },
                Absolute = false,
                Sliding = true
            };

            IAsyncPolicy<HttpResponseMessage> policy = config.AsTypeModel(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(AsyncCachePolicy<HttpResponseMessage>));
        }


        [TestMethod]
        public void AsPolicy_Absolute_ReturnsPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var provider = new Mock<IAsyncCacheProvider<HttpResponseMessage>>(MockBehavior.Strict);
            var config = new CacheConfig(provider.Object)
            {
                Time = new CacheTime { Days = 2, Hours = 0, Minutes = 5, Seconds = 0 },
                Absolute = true,
                Sliding = false
            };

            IAsyncPolicy<HttpResponseMessage> policy = config.AsTypeModel(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(AsyncCachePolicy<HttpResponseMessage>));
        }
    }
}
