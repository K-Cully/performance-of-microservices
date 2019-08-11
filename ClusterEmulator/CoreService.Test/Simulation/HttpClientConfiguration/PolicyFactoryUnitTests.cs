using CoreService.Simulation.HttpClientConfiguration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using Polly.Retry;
using System;

namespace CoreService.Test.Simulation.HttpClientConfiguration
{
    [TestClass]
    public class PolicyFactoryUnitTests
    {
        [TestMethod]
        public void Create_WithNullName_ThrowsException()
        {
            var factory = new PolicyFactory();

            Assert.ThrowsException<ArgumentException>(
                () => factory.Create(null), "Create should throw.");
        }


        [TestMethod]
        public void Create_WithNonJsonData_ReturnsNull()
        {
            string setting = "???";
            var factory = new PolicyFactory();

            IAsyncPolicy policy = factory.Create(setting);

            Assert.IsNull(policy, "IAsyncPolicy should be null");
        }


        [TestMethod]
        public void Create_WithErronousSetting_ReturnsNull()
        {
            string setting = "{ }";
            var factory = new PolicyFactory();

            IAsyncPolicy policy = factory.Create(setting);

            Assert.IsNull(policy, "IAsyncPolicy should be null");
        }


        [TestMethod]
        public void Create_WithUnknownType_Throws()
        {
            string setting = "{ type : 'JunkConfiguration', policy : {  } }";
            var factory = new PolicyFactory();

            Assert.ThrowsException<InvalidOperationException>(
                () => factory.Create(setting), "Create should throw");
        }


        [TestMethod]
        public void Create_WithMissingPolicy_Throws()
        {
            string setting = "{ type : 'RetryConfig' }";
            var factory = new PolicyFactory();

            Assert.ThrowsException<InvalidOperationException>(
                () => factory.Create(setting), "Create should throw");
        }


        [TestMethod]
        public void Create_WithInvalidPolicy_ReturnsNull()
        {
            string setting = "{ type : 'RetryConfig', policy : {  } }";
            var factory = new PolicyFactory();

            IAsyncPolicy policy = factory.Create(setting);

            Assert.IsNull(policy, "IAsyncPolicy should be null");
        }


        [TestMethod]
        public void Create_WithValidSetting_ReturnsPolicy()
        {
            string setting = "{ type : 'RetryConfig', policy : { retries : 1, delays : [ 2 ] } }";
            var factory = new PolicyFactory();

            IAsyncPolicy policy = factory.Create(setting);

            Assert.IsNotNull(policy, "IAsyncPolicy should not be null");
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy), "IAsyncPolicy should be a RetryPolicy instance");
        }
    }
}
