using CoreService.Model;
using CoreService.Simulation.Core;
using CoreService.Simulation.Processors;
using CoreService.Simulation.Steps;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreService.Test.Simulation.Core
{
    [TestClass]
    public class EngineUnitTests
    {
        [TestMethod]
        [TestCategory("Input")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Throws_WhenPassedNullRegistry()
        {
            var logger = new Mock<ILogger<Engine>>(MockBehavior.Loose);
            _ = new Engine(logger.Object, null);
        }


        [TestMethod]
        [TestCategory("Input")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Throws_WhenPassedNullLogger()
        {
            var registry = new Mock<IRegistry>(MockBehavior.Strict);
            _ = new Engine(null, registry.Object);
        }


        [TestMethod]
        [TestCategory("Input")]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ProcessRequestAsync_Throws_WhenPassedNull()
        {
            var logger = new Mock<ILogger<Engine>>(MockBehavior.Loose);
            var registry = new Mock<IRegistry>(MockBehavior.Strict);
            Engine engine = new Engine(logger.Object, registry.Object);

            await engine.ProcessRequestAsync(null).ConfigureAwait(false);
        }


        [TestMethod]
        [TestCategory("Input")]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ProcessRequestAsync_Throws_WhenPassedEmpty()
        {
            var logger = new Mock<ILogger<Engine>>(MockBehavior.Loose);
            var registry = new Mock<IRegistry>(MockBehavior.Strict);
            Engine engine = new Engine(logger.Object, registry.Object);

            await engine.ProcessRequestAsync(string.Empty).ConfigureAwait(false);
        }


        [TestMethod]
        [TestCategory("Functional")]
        [ExpectedException(typeof(NullReferenceException))]
        public async Task ProcessRequestAsync_Throws_WhenProcessorIsNull()
        {
            string name = "test";
            var registry = new Mock<IRegistry>(MockBehavior.Strict);
            registry.Setup(reg => reg.GetProcessor(name)).Returns<string>(null);
            var logger = new Mock<ILogger<Engine>>(MockBehavior.Loose);
            Engine engine = new Engine(logger.Object, registry.Object);

            await engine.ProcessRequestAsync(name).ConfigureAwait(false);
        }


        [TestMethod]
        [TestCategory("Functional")]
        [ExpectedException(typeof(NullReferenceException))]
        public async Task ProcessRequestAsync_Throws_WhenStepIsNull()
        {
            string name = "test";
            IProcessor processor = defaultProcessor;
            processor.Steps = new List<string> { null };

            var registry = new Mock<IRegistry>(MockBehavior.Strict);
            registry.Setup(reg => reg.GetProcessor(name))
                .Returns<string>((n) => processor);
            registry.Setup(reg => reg.GetStep(It.IsAny<string>()))
                .Returns<string>(null);

            var logger = new Mock<ILogger<Engine>>(MockBehavior.Loose);
            Engine engine = new Engine(logger.Object, registry.Object);

            await engine.ProcessRequestAsync(name).ConfigureAwait(false);
        }


        [TestMethod]
        [TestCategory("Functional")]
        public async Task ProcessRequestAsync_ReturnsOkayObjectResult_WhenStepsAreSuccessful()
        {
            string processorName = "processor";
            string stepName = "step";
            int payloadSize = 200;
            int latency = 300;

            DateTime start = DateTime.Now;

            IProcessor processor = defaultProcessor;
            processor.Steps = new List<string> { stepName, stepName };
            processor.SuccessPayloadSize = payloadSize;
            processor.IngressLatencyMilliseconds = latency;

            Mock<IStep> stepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.Setup(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.Success);

            var registry = new Mock<IRegistry>(MockBehavior.Strict);
            registry.Setup(reg => reg.GetProcessor(processorName))
                .Returns<string>(n => processor);
            registry.Setup(reg => reg.GetStep(It.IsAny<string>()))
                .Returns<string>(n => stepMock.Object);

            var logger = new Mock<ILogger<Engine>>(MockBehavior.Loose);
            Engine engine = new Engine(logger.Object, registry.Object);

            IActionResult result = await engine.ProcessRequestAsync(processorName).ConfigureAwait(false);

            Assert.IsTrue(DateTime.Now.Subtract(start).TotalMilliseconds > latency, "Latency should be applied correctly");
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(OkObjectResult), "Result should be OkObjectResult");
            OkObjectResult objectResult = result as OkObjectResult;
            Assert.IsInstanceOfType(objectResult.Value, typeof(SuccessResponse), "Result value should be a SuccessResponse");
            SuccessResponse value = objectResult.Value as SuccessResponse;

            var resultList = processor.SuccessPayload.Result as List<string>;
            Assert.IsNotNull(resultList);
            Assert.AreEqual(2, resultList.Count, "Success result should be the correct length");
            Assert.AreEqual(64, resultList[0].Length, "Success payload should be the correct size");
            Assert.AreEqual(36, resultList[1].Length, "Success payload should be the correct size");
        }


        [TestMethod]
        [TestCategory("Functional")]
        public async Task ProcessRequestAsync_ReturnsInternalServerError_WhenStepReturnsUnexpected()
        {
            string processorName = "processor";
            string okayStepName = "step";
            string failStepName = "nope";
            int payloadSize = 42;
            int errorPayloadSize = 7;

            IProcessor processor = defaultProcessor;
            processor.Steps = new List<string> { okayStepName, failStepName };
            processor.SuccessPayloadSize = payloadSize;
            processor.ErrorPayloadSize = errorPayloadSize;

            Mock<IStep> stepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.Setup(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.Success);
            Mock<IStep> failStepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.Setup(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.Fail);

            var registry = new Mock<IRegistry>(MockBehavior.Strict);
            registry.Setup(reg => reg.GetProcessor(processorName))
                .Returns<string>(n => processor);
            registry.Setup(reg => reg.GetStep(okayStepName))
                .Returns<string>(n => stepMock.Object);
            registry.Setup(reg => reg.GetStep(failStepName))
                .Returns<string>(n => failStepMock.Object);

            var logger = new Mock<ILogger<Engine>>(MockBehavior.Loose);
            Engine engine = new Engine(logger.Object, registry.Object);

            IActionResult result = await engine.ProcessRequestAsync(processorName).ConfigureAwait(false);

            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ObjectResult), "Result should be ObjectResult");
            ObjectResult objectResult = result as ObjectResult;
            Assert.IsTrue(objectResult.StatusCode.HasValue, "Status code should not be null");
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode.Value,
                "Status code should be InternalServerError");
            Assert.IsInstanceOfType(objectResult.Value, typeof(ErrorResponse), "Result value should be an ErrorResponse");
            ErrorResponse value = objectResult.Value as ErrorResponse;
            Assert.IsFalse(string.IsNullOrEmpty(value.Error), "Error should be initialized with a valid string");
            Assert.AreEqual(errorPayloadSize / 2, value.Error.Length, "Error should be of the correct length");
        }


        [TestMethod]
        [TestCategory("Functional")]
        public async Task ProcessRequestAsync_ReturnsImATeapotError_WhenStepsFail()
        {
            string processorName = "processor";
            string okayStepName = "step";
            string failStepName = "nope";
            int payloadSize = 42;
            int errorPayloadSize = 7;

            IProcessor processor = defaultProcessor;
            processor.Steps = new List<string> { okayStepName, failStepName };
            processor.SuccessPayloadSize = payloadSize;
            processor.ErrorPayloadSize = errorPayloadSize;

            Mock<IStep> stepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.Setup(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.Success);
            Mock<IStep> failStepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.Setup(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.SimulatedFail);

            Mock<IRegistry> registry = new Mock<IRegistry>(MockBehavior.Strict);
            registry.Setup(reg => reg.GetProcessor(processorName))
                .Returns<string>(n => processor);
            registry.Setup(reg => reg.GetStep(okayStepName))
                .Returns<string>(n => stepMock.Object);
            registry.Setup(reg => reg.GetStep(failStepName))
                .Returns<string>(n => failStepMock.Object);

            var logger = new Mock<ILogger<Engine>>(MockBehavior.Loose);
            Engine engine = new Engine(logger.Object, registry.Object);

            IActionResult result = await engine.ProcessRequestAsync(processorName).ConfigureAwait(false);

            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ObjectResult), "Result should be ObjectResult");
            ObjectResult objectResult = result as ObjectResult;
            Assert.IsTrue(objectResult.StatusCode.HasValue, "Status code should not be null");
            Assert.AreEqual(StatusCodes.Status418ImATeapot, objectResult.StatusCode.Value,
                "Status code should be InternalServerError");
            Assert.IsInstanceOfType(objectResult.Value, typeof(ErrorResponse), "Result value should be an ErrorResponse");
            ErrorResponse value = objectResult.Value as ErrorResponse;
            Assert.IsFalse(string.IsNullOrEmpty(value.Error), "Error should be initialized with a valid string");
            Assert.AreEqual(errorPayloadSize / 2, value.Error.Length, "Erorr should be of the correct length");
        }


        private readonly Processor defaultProcessor = new Processor
        {
            Steps = new List<string>(),
            ErrorPayloadSize = 0,
            SuccessPayloadSize = 0,
            IngressLatencyMilliseconds = 0
        };
    }
}
