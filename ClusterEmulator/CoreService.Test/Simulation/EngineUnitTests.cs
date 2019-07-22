using CoreService.Simulation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

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
        public void ProcessRequest_Throws_WhenProcessorIsNull()
        {
            string name = "test";
            Mock<IRegistry> registryMock = new Mock<IRegistry>(MockBehavior.Strict);
            registryMock.Setup(reg => reg.GetProcessor(name)).Returns<string>(null);
            Engine engine = new Engine(registryMock.Object);

            ActionResult<string> result = engine.ProcessRequest(name);
        }


        [TestMethod]
        [TestCategory("Functional")]
        [ExpectedException(typeof(NullReferenceException))]
        public void ProcessRequest_Throws_WhenStepIsNull()
        {
            string name = "test";
            Mock<IRegistry> registryMock = new Mock<IRegistry>(MockBehavior.Strict);
            IProcessor processor = defaultProcessor;
            processor.Steps = new List<string> { null };
            processor.Name = name;

            registryMock.Setup(reg => reg.GetProcessor(name))
                .Returns<string>((n) => processor);
            registryMock.Setup(reg => reg.GetStep(It.IsAny<string>()))
                .Returns<string>(null);
            Engine engine = new Engine(registryMock.Object);

            ActionResult<string> result = engine.ProcessRequest(name);
        }


        [TestMethod]
        [TestCategory("Functional")]
        public void ProcessRequest_ExecutesSuccessfully_WhenStepsAreSuccessful()
        {
            string name = "test";
            Mock<IRegistry> registryMock = new Mock<IRegistry>(MockBehavior.Strict);
            IProcessor processor = defaultProcessor;
            processor.Steps = new List<string> { "step1" };
            processor.Name = name;

            // TODO: return valid step
            registryMock.Setup(reg => reg.GetProcessor(name))
                .Returns<string>((n) => processor);
            registryMock.Setup(reg => reg.GetStep(It.IsAny<string>()))
                .Returns<string>(null);
            Engine engine = new Engine(registryMock.Object);

            // TODO: verify result
            ActionResult<string> result = engine.ProcessRequest(name);
        }


        private readonly Processor defaultProcessor = new Processor
            {
                Name = string.Empty,
                Steps = new List<string>(),
                ErrorPayloadSize = 0,
                SuccessPayloadSize = 0,
                IngressLatencyMilliseconds = 0
            };
    }
}
