using CoreService.Simulation;
using CoreService.Simulation.Steps;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreService.Test.Simulation
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
        public async Task ProcessRequest_Throws_WhenPassedNullAsync()
        {
            Mock<IRegistry> registryMock = new Mock<IRegistry>(MockBehavior.Strict);
            Engine engine = new Engine(registryMock.Object);

            await engine.ProcessRequest(null).ConfigureAwait(false);
        }


        [TestMethod]
        [TestCategory("Input")]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ProcessRequest_Throws_WhenPassedEmptyAsync()
        {
            Mock<IRegistry> registryMock = new Mock<IRegistry>(MockBehavior.Strict);
            Engine engine = new Engine(registryMock.Object);

            await engine.ProcessRequest(string.Empty).ConfigureAwait(false);
        }


        [TestMethod]
        [TestCategory("Functional")]
        [ExpectedException(typeof(NullReferenceException))]
        public async Task ProcessRequest_Throws_WhenProcessorIsNullAsync()
        {
            string name = "test";
            Mock<IRegistry> registryMock = new Mock<IRegistry>(MockBehavior.Strict);
            registryMock.Setup(reg => reg.GetProcessor(name)).Returns<string>(null);
            Engine engine = new Engine(registryMock.Object);

            await engine.ProcessRequest(name).ConfigureAwait(false);
        }


        [TestMethod]
        [TestCategory("Functional")]
        [ExpectedException(typeof(NullReferenceException))]
        public async Task ProcessRequest_Throws_WhenStepIsNullAsync()
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

           await engine.ProcessRequest(name).ConfigureAwait(false);
        }


        [TestMethod]
        [TestCategory("Functional")]
        public async Task ProcessRequest_ReturnsOkayObjectResult_WhenStepsAreSuccessfulAsync()
        {
            string processorName = "processor";
            string stepName = "step";
            int payloadSize = 42;

            IProcessor processor = defaultProcessor;
            processor.Steps = new List<string> { stepName, stepName };
            processor.Name = processorName;
            processor.SuccessPayloadSize = payloadSize;

            Mock<IStep> stepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.Setup(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.Success);

            Mock<IRegistry> registryMock = new Mock<IRegistry>(MockBehavior.Strict);
            registryMock.Setup(reg => reg.GetProcessor(processorName))
                .Returns<string>(n => processor);
            registryMock.Setup(reg => reg.GetStep(It.IsAny<string>()))
                .Returns<string>(n => stepMock.Object);

            Engine engine = new Engine(registryMock.Object);

            IActionResult result = await engine.ProcessRequest(processorName).ConfigureAwait(false);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(OkObjectResult), "Result should be OkObjectResult");
            OkObjectResult objectResult = result as OkObjectResult;
            Assert.IsInstanceOfType(objectResult.Value, typeof(string), "Result value should be a string");
            string value = objectResult.Value as string;
            Assert.IsFalse(string.IsNullOrEmpty(value), "Value should be initialized with a valid string");
            Assert.AreEqual(value.Length, payloadSize, "Value should be of the correct length");
        }


        [TestMethod]
        [TestCategory("Functional")]
        public async Task ProcessRequest_ReturnsInternalServerError_WhenStepsFailAsync()
        {
            string processorName = "processor";
            string okayStepName = "step";
            string failStepName = "nope";
            int payloadSize = 42;
            int errorPayloadSize = 7;

            IProcessor processor = defaultProcessor;
            processor.Steps = new List<string> { okayStepName, failStepName };
            processor.Name = processorName;
            processor.SuccessPayloadSize = payloadSize;
            processor.ErrorPayloadSize = errorPayloadSize;

            Mock<IStep> stepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.Setup(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.Success);
            Mock<IStep> failStepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.Setup(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.Fail);

            Mock<IRegistry> registryMock = new Mock<IRegistry>(MockBehavior.Strict);
            registryMock.Setup(reg => reg.GetProcessor(processorName))
                .Returns<string>(n => processor);
            registryMock.Setup(reg => reg.GetStep(okayStepName))
                .Returns<string>(n => stepMock.Object);
            registryMock.Setup(reg => reg.GetStep(failStepName))
                .Returns<string>(n => failStepMock.Object);

            Engine engine = new Engine(registryMock.Object);

            IActionResult result = await engine.ProcessRequest(processorName).ConfigureAwait(false);

            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ObjectResult), "Result should be ObjectResult");
            ObjectResult objectResult = result as ObjectResult;
            Assert.IsTrue(objectResult.StatusCode.HasValue, "Status code should not be null");
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode.Value, "Status code should be InternalServerError");
            Assert.IsInstanceOfType(objectResult.Value, typeof(string), "Result value should be a string");
            Assert.IsInstanceOfType(objectResult.Value, typeof(string), "Result value should be a string");
            string value = objectResult.Value as string;
            Assert.IsFalse(string.IsNullOrEmpty(value), "Value should be initialized with a valid string");
            Assert.AreEqual(value.Length, errorPayloadSize, "Value should be of the correct length");
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
