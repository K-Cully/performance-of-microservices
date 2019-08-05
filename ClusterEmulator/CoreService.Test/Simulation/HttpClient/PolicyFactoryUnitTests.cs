using CoreService.Simulation.HttpClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using Polly.Retry;
using System;

namespace CoreService.Test.Simulation.HttpClient
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

            IsPolicy policy = factory.Create(setting);

            Assert.IsNull(policy, "Policy should be null");
        }


        [TestMethod]
        public void Create_WithErronousSetting_ReturnsNull()
        {
            string setting = "{ }";
            var factory = new PolicyFactory();

            IsPolicy policy = factory.Create(setting);

            Assert.IsNull(policy, "Policy should be null");
        }


        [TestMethod]
        public void Create_WithUnknownType_ThrowsException()
        {
            string setting = "{ type : 'JunkConfiguration', policy : {  } }";
            var factory = new PolicyFactory();

            Assert.ThrowsException<InvalidOperationException>(
                () => factory.Create(setting), "Create should throw");
        }


        [TestMethod]
        public void Create_WithInvalidSetting_ReturnsNull()
        {
            string setting = "{ type : 'RetryConfiguration', policy : {  } }";
            var factory = new PolicyFactory();

            IsPolicy policy = factory.Create(setting);

            Assert.IsNull(policy, "Policy should be null");
        }


        [TestMethod]
        public void Create_WithValidSetting_ReturnsPolicy()
        {
            string setting = "{ type : 'RetryConfiguration', policy : { retries : 1, delays : [ 2 ] } }";
            var factory = new PolicyFactory();

            IsPolicy policy = factory.Create(setting);

            Assert.IsNotNull(policy, "Policy should not be null");
            Assert.IsInstanceOfType(policy, typeof(RetryPolicy), "Policy should be a RetryPolicy instance");
        }
    }
}
