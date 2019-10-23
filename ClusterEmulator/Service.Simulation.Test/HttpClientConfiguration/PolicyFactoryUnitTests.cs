using ClusterEmulator.Service.Simulation.Core;
using ClusterEmulator.Service.Simulation.HttpClientConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Polly;
using Polly.Retry;
using System;
using System.Net.Http;

namespace ClusterEmulator.Service.Simulation.Test.HttpClientConfiguration
{
    [TestClass]
    public class PolicyFactoryUnitTests
    {
        [TestMethod]
        public void Constructor_WithNullLogger_ThrowsException()
        {
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            Assert.ThrowsException<ArgumentNullException>(
                () => new NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>(
                    null, loggerFactory.Object), "Constructor should throw.");
        }


        [TestMethod]
        public void Constructor_WithNullLoggerFactory_ThrowsException()
        {
            
            var logger = new Mock<ILogger<NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>>>(MockBehavior.Loose);

            Assert.ThrowsException<ArgumentNullException>(
                () => new NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>(
                    logger.Object, null), "Constructor should throw.");
        }


        [TestMethod]
        public void Create_WithNullName_ThrowsException()
        {
            var logger = new Mock<ILogger<NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var factory = new NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>(
                logger.Object, loggerFactory.Object);

            Assert.ThrowsException<ArgumentException>(
                () => factory.Create(null), "Create should throw.");
        }


        [TestMethod]
        public void Create_WithNonJsonData_ReturnsNull()
        {
            string setting = "???";
            var logger = new Mock<ILogger<NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var factory = new NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>(
                logger.Object, loggerFactory.Object);

            IAsyncPolicy<HttpResponseMessage> policy = factory.Create(setting);

            Assert.IsNull(policy, "IAsyncPolicy<HttpResponseMessage> should be null");
        }


        [TestMethod]
        public void Create_WithErronousSetting_ReturnsNull()
        {
            string setting = "{ }";
            var logger = new Mock<ILogger<NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var factory = new NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>(
                logger.Object, loggerFactory.Object);

            IAsyncPolicy<HttpResponseMessage> policy = factory.Create(setting);

            Assert.IsNull(policy, "IAsyncPolicy<HttpResponseMessage> should be null");
        }


        [TestMethod]
        public void Create_WithUnknownType_Throws()
        {
            string setting = "{ type : 'JunkConfiguration', value : {  } }";
            var logger = new Mock<ILogger<NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var factory = new NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>(
                logger.Object, loggerFactory.Object);

            Assert.ThrowsException<InvalidOperationException>(
                () => factory.Create(setting), "Create should throw");
        }


        [TestMethod]
        public void Create_WithMissingPolicyValue_Throws()
        {
            string setting = "{ type : 'RetryConfig' }";
            var logger = new Mock<ILogger<NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var factory = new NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>(
                logger.Object, loggerFactory.Object);

            Assert.ThrowsException<InvalidOperationException>(
                () => factory.Create(setting), "Create should throw");
        }


        [TestMethod]
        public void Create_WithInvalidPolicy_ReturnsNull()
        {
            string setting = "{ type : 'RetryConfig', value : {  } }";
            var logger = new Mock<ILogger<NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var factory = new NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>(
                logger.Object, loggerFactory.Object);

            IAsyncPolicy<HttpResponseMessage> policy = factory.Create(setting);

            Assert.IsNull(policy, "IAsyncPolicy<HttpResponseMessage> should be null");
        }


        [TestMethod]
        public void Create_WithValidSetting_ReturnsPolicy()
        {
            string setting = "{ type : 'RetryConfig', value : { retries : 1, delays : [ 2 ] } }";
            var logger = new Mock<ILogger<NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            // Mock string implementation, called by extenstion method that takes Type
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            var factory = new NestedConfigFactory<IPolicyConfiguration, IAsyncPolicy<HttpResponseMessage>>(
                logger.Object, loggerFactory.Object);

            IAsyncPolicy<HttpResponseMessage> policy = factory.Create(setting);

            Assert.IsNotNull(policy, "IAsyncPolicy<HttpResponseMessage> should not be null");
            Assert.IsInstanceOfType(policy, typeof(AsyncRetryPolicy<HttpResponseMessage>),
                "IAsyncPolicy<HttpResponseMessage> should be a RetryPolicy instance");
        }
    }
}
