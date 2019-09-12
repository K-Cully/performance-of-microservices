using CoreService.Simulation.HttpClientConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Polly;
using Polly.Bulkhead;
using System;
using System.Net.Http;

namespace CoreService.Test.Simulation.HttpClientConfiguration
{
    [TestClass]
    public class BulkheadConfigUnitTests
    {
        [TestMethod]
        public void Deserialization_AllData_CreatesValidInstance()
        {
            BulkheadConfig config = JsonConvert.DeserializeObject<BulkheadConfig>(
                "{ bandwidth : 10, queueLength : 5 }");

            Assert.IsNotNull(config);
            Assert.AreEqual(10, config.MaxParallelization);
            Assert.IsTrue(config.MaxQueuingActions.HasValue);
            Assert.AreEqual(5, config.MaxQueuingActions.Value);
        }


        [TestMethod]
        public void Deserialization_ValidData_CreatesValidInstance()
        {
            BulkheadConfig config = JsonConvert.DeserializeObject<BulkheadConfig>(
                "{ bandwidth : 10 }");

            Assert.IsNotNull(config);
            Assert.AreEqual(10, config.MaxParallelization);
            Assert.IsFalse(config.MaxQueuingActions.HasValue);
            Assert.IsNull(config.MaxQueuingActions);
        }


        [TestMethod]
        public void Deserialization_InvalidData_Throws()
        {
            Assert.ThrowsException<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<BulkheadConfig>("{ }"));
        }


        [TestMethod]
        public void AsPolicy_LoggerNull_Throws()
        {
            var config = new BulkheadConfig
            {
                MaxParallelization = 50,
                MaxQueuingActions = 10
            };

            Assert.ThrowsException<ArgumentNullException>(
                () => config.AsPolicy(null));
        }


        [TestMethod]
        public void AsPolicy_InvalidMaxParallelization_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new BulkheadConfig
            {
                MaxParallelization = -1,
                MaxQueuingActions = 10
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => config.AsPolicy(logger.Object));
        }


        [TestMethod]
        public void AsPolicy_InvalidMaxQueuingActions_Throws()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new BulkheadConfig
            {
                MaxParallelization = 20,
                MaxQueuingActions = -1
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => config.AsPolicy(logger.Object));
        }


        [TestMethod]
        public void AsPolicy_AllValuesInitialized_ReturnsPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new BulkheadConfig
            {
                MaxParallelization = 20,
                MaxQueuingActions = 10
            };

            IAsyncPolicy<HttpResponseMessage> policy = config.AsPolicy(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(BulkheadPolicy<HttpResponseMessage>));
        }


        [TestMethod]
        public void AsPolicy_RequiredValuesInitialized_ReturnsPolicy()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            var config = new BulkheadConfig
            {
                MaxParallelization = 20,
                MaxQueuingActions = null
            };

            IAsyncPolicy<HttpResponseMessage> policy = config.AsPolicy(logger.Object);

            Assert.IsNotNull(policy);
            Assert.IsInstanceOfType(policy, typeof(BulkheadPolicy<HttpResponseMessage>));
        }
    }
}
