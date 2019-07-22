using CoreService.Simulation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace CoreService.Test
{
    [TestClass]
    public class EngineUnitTests
    {
        [TestMethod]
        [TestCategory("Input")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Throws_WhenPassedNull()
        {
            _ = new Engine(null);
        }


        [TestMethod]
        [TestCategory("Input")]
        [ExpectedException(typeof(ArgumentException))]
        public void ProcessRequest_Throws_WhenPassedNull()
        {
            Mock<IRegistry> registryMock = new Mock<IRegistry>(MockBehavior.Strict);
            Engine engine = new Engine(registryMock.Object);

            engine.ProcessRequest(null);
        }


        [TestMethod]
        [TestCategory("Input")]
        [ExpectedException(typeof(ArgumentException))]
        public void ProcessRequest_Throws_WhenPassedEmpty()
        {
            Mock<IRegistry> registryMock = new Mock<IRegistry>(MockBehavior.Strict);
            Engine engine = new Engine(registryMock.Object);

            engine.ProcessRequest(string.Empty);
        }


        [TestMethod]
        [TestCategory("Functional")]
        [ExpectedException(typeof(NullReferenceException))]
        public void ProcessRequest_Throws_WhenRegistryReturnsNull()
        {
            string name = "test";
            Mock<IRegistry> registryMock = new Mock<IRegistry>(MockBehavior.Strict);
            registryMock.Setup(reg => reg.GetProcessor(name)).Returns<string>(null);
            Engine engine = new Engine(registryMock.Object);

            ActionResult<string> result = engine.ProcessRequest(name);
        }
    }
}
