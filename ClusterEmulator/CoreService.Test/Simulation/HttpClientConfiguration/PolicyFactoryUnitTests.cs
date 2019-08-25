using CoreService.Simulation.HttpClientConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Polly;
using Polly.Retry;
using System;

namespace CoreService.Test.Simulation.HttpClientConfiguration
{
    [TestClass]
    public class PolicyFactoryUnitTests
    {
        [TestMethod]
        public void Constructor_WithNullLogger_ThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new PolicyFactory(null), "Constructor should throw.");
        }


        [TestMethod]
        public void Create_WithNullName_ThrowsException()
        {
            var logger = new Mock<ILogger<PolicyFactory>>(MockBehavior.Loose);
            var factory = new PolicyFactory(logger.Object);

            Assert.ThrowsException<ArgumentException>(
                () => factory.Create(null), "Create should throw.");
        }


        [TestMethod]
        public void Create_WithNonJsonData_ReturnsNull()
        {
            string setting = "???";
            var logger = new Mock<ILogger<PolicyFactory>>(MockBehavior.Loose);
            var factory = new PolicyFactory(logger.Object);

            IAsyncPolicy policy = factory.Create(setting);

            Assert.IsNull(policy, "IAsyncPolicy should be null");
        }


        [TestMethod]
        public void Create_WithErronousSetting_ReturnsNull()
        {
            string setting = "{ }";
            var logger = new Mock<ILogger<PolicyFactory>>(MockBehavior.Loose);
            var factory = new PolicyFactory(logger.Object);

            IAsyncPolicy policy = factory.Create(setting);

            Assert.IsNull(policy, "IAsyncPolicy should be null");
        }


        [TestMethod]
        public void Create_WithUnknownType_Throws()
        {
            string setting = "{ type : 'JunkConfiguration', policy : {  } }";
            var logger = new Mock<ILogger<PolicyFactory>>(MockBehavior.Loose);
            var factory = new PolicyFactory(logger.Object);

            Assert.ThrowsException<InvalidOperationException>(
                () => factory.Create(setting), "Create should throw");
        }


        [TestMethod]
        public void Create_WithMissingPolicy_Throws()
        {
            string setting = "{ type : 'RetryConfig' }";
            var logger = new Mock<ILogger<PolicyFactory>>(MockBehavior.Loose);
            var factory = new PolicyFactory(logger.Object);

            Assert.ThrowsException<InvalidOperationException>(
                () => factory.Create(setting), "Create should throw");
        }


        [TestMethod]
        public void Create_WithInvalidPolicy_ReturnsNull()
        {
            string setting = "{ type : 'RetryConfig', policy : {  } }";
            var logger = new Mock<ILogger<PolicyFactory>>(MockBehavior.Loose);
            var factory = new PolicyFactory(logger.Object);

            IAsyncPolicy policy = factory.Create(setting);

            Assert.IsNull(policy, "IAsyncPolicy should be null");
        }


        [TestMethod]
        public void Create_WithValidSetting_ReturnsPolicy()
        {
            string setting = "{ type : 'RetryConfig', policy : { retries : 1, delays : [ 2 ] } }";
            var logger = new Mock<ILogger<PolicyFactory>>(MockBehavior.Loose);
            var factory = new PolicyFactory(logger.Object);

            IAsyncPolicy policy = factory.Create(setting);

            Assert.IsNotNull(policy, "IAsyncPolicy should not be null");
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy), "IAsyncPolicy should be a RetryPolicy instance");
        }
    }
}
