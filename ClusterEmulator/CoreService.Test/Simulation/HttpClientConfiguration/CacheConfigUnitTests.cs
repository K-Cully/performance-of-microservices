using CoreService.Simulation.HttpClientConfiguration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreService.Test.Simulation.HttpClientConfiguration
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
            var config = new CacheConfig
            {
                Time = new CacheTime { Days = 0, Hours = 0, Minutes = 5, Seconds = 0 },
                Absolute = false,
                Sliding = false
            };

            Assert.ThrowsException<ArgumentNullException>(
                () => config.AsPolicy(null));
        }

        // TODO: success cases
    }
}
