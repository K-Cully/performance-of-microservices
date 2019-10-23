using ClusterEmulator.Service.Models;
using ClusterEmulator.Service.Simulation.Core;
using ClusterEmulator.Service.Simulation.Processors;
using ClusterEmulator.Service.Simulation.Steps;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClusterEmulator.Service.Simulation.Test.Core
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
            registry.Setup(reg => reg.GetRequestProcessor(name)).Returns<string>(null);
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
            IRequestProcessor processor = defaultRequestProcessor;
            processor.Steps = new List<string> { null };

            var registry = new Mock<IRegistry>(MockBehavior.Strict);
            registry.Setup(reg => reg.GetRequestProcessor(name))
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

            IRequestProcessor processor = defaultRequestProcessor;
            processor.Steps = new List<string> { stepName, stepName };
            processor.SuccessPayloadSize = payloadSize;
            processor.IngressLatencyMilliseconds = latency;

            Mock<IStep> stepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.Setup(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.Success);
            stepMock.Setup(step => step.FailOnParallelFailures)
                .Returns(GroupClause.Undefined);
            stepMock.Setup(step => step.ParallelCount)
                .Returns<uint?>(null);

            var registry = new Mock<IRegistry>(MockBehavior.Strict);
            registry.Setup(reg => reg.GetRequestProcessor(processorName))
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

            IRequestProcessor processor = defaultRequestProcessor;
            processor.Steps = new List<string> { okayStepName, failStepName };
            processor.SuccessPayloadSize = payloadSize;
            processor.ErrorPayloadSize = errorPayloadSize;

            Mock<IStep> stepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.Setup(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.Success);
            stepMock.Setup(step => step.FailOnParallelFailures)
                .Returns(GroupClause.Undefined);
            stepMock.Setup(step => step.ParallelCount)
                .Returns<uint?>(null);
            Mock<IStep> failStepMock = new Mock<IStep>(MockBehavior.Strict);
            failStepMock.Setup(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.Fail);
            failStepMock.Setup(step => step.FailOnParallelFailures)
                .Returns(GroupClause.Undefined);
            failStepMock.Setup(step => step.ParallelCount)
                .Returns<uint?>(null);

            var registry = new Mock<IRegistry>(MockBehavior.Strict);
            registry.Setup(reg => reg.GetRequestProcessor(processorName))
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

            IRequestProcessor processor = defaultRequestProcessor;
            processor.Steps = new List<string> { okayStepName, failStepName };
            processor.SuccessPayloadSize = payloadSize;
            processor.ErrorPayloadSize = errorPayloadSize;

            Mock<IStep> stepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.Setup(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.Success);
            stepMock.Setup(step => step.FailOnParallelFailures)
                .Returns(GroupClause.Undefined);
            stepMock.Setup(step => step.ParallelCount)
                .Returns<uint?>(null);
            Mock<IStep> failStepMock = new Mock<IStep>(MockBehavior.Strict);
            failStepMock.Setup(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.SimulatedFail);
            failStepMock.Setup(step => step.FailOnParallelFailures)
                .Returns(GroupClause.Undefined);
            failStepMock.Setup(step => step.ParallelCount)
                .Returns<uint?>(null);

            Mock<IRegistry> registry = new Mock<IRegistry>(MockBehavior.Strict);
            registry.Setup(reg => reg.GetRequestProcessor(processorName))
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
                "Status code should be Status418ImATeapot");
            Assert.IsInstanceOfType(objectResult.Value, typeof(ErrorResponse), "Result value should be an ErrorResponse");
            ErrorResponse value = objectResult.Value as ErrorResponse;
            Assert.IsFalse(string.IsNullOrEmpty(value.Error), "Error should be initialized with a valid string");
            Assert.AreEqual(errorPayloadSize / 2, value.Error.Length, "Erorr should be of the correct length");
        }


        [TestMethod]
        [TestCategory("Functional")]
        public async Task ProcessRequestAsync_ParallelAllSteps_OneSuccessful_ReturnsSuccessful()
        {
            // Arrange
            string processorName = "processor";
            string stepName = "step";
            int payloadSize = 42;
            int errorPayloadSize = 7;

            IRequestProcessor processor = defaultRequestProcessor;
            processor.Steps = new List<string> { stepName };
            processor.SuccessPayloadSize = payloadSize;
            processor.ErrorPayloadSize = errorPayloadSize;

            GroupClause clause = GroupClause.All;
            uint parallelCount = 3;
            Mock<IStep> stepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.SetupSequence(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.SimulatedFail)
                .ReturnsAsync(ExecutionStatus.Success)
                .ReturnsAsync(ExecutionStatus.Fail);
            stepMock.Setup(step => step.FailOnParallelFailures).Returns(clause);
            stepMock.Setup(step => step.ParallelCount).Returns(parallelCount);

            Mock<IRegistry> registry = new Mock<IRegistry>(MockBehavior.Strict);
            registry.Setup(reg => reg.GetRequestProcessor(processorName)).Returns<string>(n => processor);
            registry.Setup(reg => reg.GetStep(stepName)).Returns<string>(n => stepMock.Object);

            var logger = new Mock<ILogger<Engine>>(MockBehavior.Loose);
            Engine engine = new Engine(logger.Object, registry.Object);

            // Act
            IActionResult result = await engine.ProcessRequestAsync(processorName).ConfigureAwait(false);

            // Verify
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(OkObjectResult), "Result should be OkObjectResult");
            OkObjectResult objectResult = result as OkObjectResult;
            Assert.IsInstanceOfType(objectResult.Value, typeof(SuccessResponse), "Result value should be a SuccessResponse");
        }


        [TestMethod]
        [TestCategory("Functional")]
        public async Task ProcessRequestAsync_ParallelAllSteps_AllFailures_ReturnsFirstFailure()
        {
            // Arrange
            string processorName = "processor";
            string stepName = "step";
            int payloadSize = 42;
            int errorPayloadSize = 7;

            IRequestProcessor processor = defaultRequestProcessor;
            processor.Steps = new List<string> { stepName };
            processor.SuccessPayloadSize = payloadSize;
            processor.ErrorPayloadSize = errorPayloadSize;

            GroupClause clause = GroupClause.All;
            uint parallelCount = 2;
            Mock<IStep> stepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.SetupSequence(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.SimulatedFail)
                .ReturnsAsync(ExecutionStatus.Fail);
            stepMock.Setup(step => step.FailOnParallelFailures).Returns(clause);
            stepMock.Setup(step => step.ParallelCount).Returns(parallelCount);

            Mock<IRegistry> registry = new Mock<IRegistry>(MockBehavior.Strict);
            registry.Setup(reg => reg.GetRequestProcessor(processorName)).Returns<string>(n => processor);
            registry.Setup(reg => reg.GetStep(stepName)).Returns<string>(n => stepMock.Object);

            var logger = new Mock<ILogger<Engine>>(MockBehavior.Loose);
            Engine engine = new Engine(logger.Object, registry.Object);

            // Act
            IActionResult result = await engine.ProcessRequestAsync(processorName).ConfigureAwait(false);

            // Verify
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ObjectResult), "Result should be ObjectResult");
            ObjectResult objectResult = result as ObjectResult;
            Assert.IsTrue(objectResult.StatusCode.HasValue, "Status code should not be null");
            Assert.AreEqual(StatusCodes.Status418ImATeapot, objectResult.StatusCode.Value,
                "Status code should be Status418ImATeapot");
        }


        [TestMethod]
        [TestCategory("Functional")]
        public async Task ProcessRequestAsync_ParallelNoneSteps_AllFailures_ReturnsSuccess()
        {
            // Arrange
            string processorName = "processor";
            string stepName = "step";
            int payloadSize = 42;
            int errorPayloadSize = 7;

            IRequestProcessor processor = defaultRequestProcessor;
            processor.Steps = new List<string> { stepName };
            processor.SuccessPayloadSize = payloadSize;
            processor.ErrorPayloadSize = errorPayloadSize;

            GroupClause clause = GroupClause.None;
            uint parallelCount = 3;
            Mock<IStep> stepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.SetupSequence(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.SimulatedFail)
                .ReturnsAsync(ExecutionStatus.Fail)
                .ReturnsAsync(ExecutionStatus.Fail);
            stepMock.Setup(step => step.FailOnParallelFailures).Returns(clause);
            stepMock.Setup(step => step.ParallelCount).Returns(parallelCount);

            Mock<IRegistry> registry = new Mock<IRegistry>(MockBehavior.Strict);
            registry.Setup(reg => reg.GetRequestProcessor(processorName)).Returns<string>(n => processor);
            registry.Setup(reg => reg.GetStep(stepName)).Returns<string>(n => stepMock.Object);

            var logger = new Mock<ILogger<Engine>>(MockBehavior.Loose);
            Engine engine = new Engine(logger.Object, registry.Object);

            // Act
            IActionResult result = await engine.ProcessRequestAsync(processorName).ConfigureAwait(false);

            // Verify
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(OkObjectResult), "Result should be OkObjectResult");
            OkObjectResult objectResult = result as OkObjectResult;
            Assert.IsInstanceOfType(objectResult.Value, typeof(SuccessResponse), "Result value should be a SuccessResponse");
        }


        [TestMethod]
        [TestCategory("Functional")]
        public async Task ProcessRequestAsync_ParallelAnySteps_OneSuccessful_ReturnsFirstFailure()
        {
            // Arrange
            string processorName = "processor";
            string stepName = "step";
            int payloadSize = 42;
            int errorPayloadSize = 7;

            IRequestProcessor processor = defaultRequestProcessor;
            processor.Steps = new List<string> { stepName };
            processor.SuccessPayloadSize = payloadSize;
            processor.ErrorPayloadSize = errorPayloadSize;

            GroupClause clause = GroupClause.Any;
            uint parallelCount = 3;
            Mock<IStep> stepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.SetupSequence(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.SimulatedFail)
                .ReturnsAsync(ExecutionStatus.Success)
                .ReturnsAsync(ExecutionStatus.Fail);
            stepMock.Setup(step => step.FailOnParallelFailures).Returns(clause);
            stepMock.Setup(step => step.ParallelCount).Returns(parallelCount);

            Mock<IRegistry> registry = new Mock<IRegistry>(MockBehavior.Strict);
            registry.Setup(reg => reg.GetRequestProcessor(processorName)).Returns<string>(n => processor);
            registry.Setup(reg => reg.GetStep(stepName)).Returns<string>(n => stepMock.Object);

            var logger = new Mock<ILogger<Engine>>(MockBehavior.Loose);
            Engine engine = new Engine(logger.Object, registry.Object);

            // Act
            IActionResult result = await engine.ProcessRequestAsync(processorName).ConfigureAwait(false);

            // Verify
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsInstanceOfType(result, typeof(ObjectResult), "Result should be ObjectResult");
            ObjectResult objectResult = result as ObjectResult;
            Assert.IsTrue(objectResult.StatusCode.HasValue, "Status code should not be null");
            Assert.AreEqual(StatusCodes.Status418ImATeapot, objectResult.StatusCode.Value,
                "Status code should be Status418ImATeapot");
        }


        [TestMethod]
        [TestCategory("Functional")]
        public async Task ProcessStartupActionsAsync_ExecutesCorrectly_WhenStepsAreSuccessful()
        {
            // Arrange
            string stepName = "step";

            DateTime start = DateTime.Now;

            IStartupProcessor processor = defaultStartupProcessor;
            processor.Steps = new List<string> { stepName, stepName };
            processor.Asynchronous = false;

            Mock<IStep> stepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.Setup(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.Success);
            stepMock.Setup(step => step.FailOnParallelFailures)
                .Returns(GroupClause.Undefined);
            stepMock.Setup(step => step.ParallelCount)
                .Returns<uint?>(null);

            var registry = new Mock<IRegistry>(MockBehavior.Strict);
            registry.Setup(reg => reg.GetStartupProcessors())
                .Returns(new List<IStartupProcessor>() { processor });
            registry.Setup(reg => reg.GetStep(It.IsAny<string>()))
                .Returns<string>(n => stepMock.Object);

            var logger = new Mock<ILogger<Engine>>(MockBehavior.Loose);
            Engine engine = new Engine(logger.Object, registry.Object);

            // Act
            await engine.ProcessStartupActionsAsync().ConfigureAwait(false);

            // Verify
            Assert.IsTrue(true);
        }


        [TestMethod]
        [TestCategory("Functional")]
        public async Task ProcessStartupActionsAsync_Throws_WhenStepsFail()
        {
            // Arrange
            string stepName = "step";

            DateTime start = DateTime.Now;

            IStartupProcessor processor = defaultStartupProcessor;
            processor.Steps = new List<string> { stepName, stepName };
            processor.Asynchronous = false;

            Mock<IStep> stepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.Setup(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.Fail);
            stepMock.Setup(step => step.FailOnParallelFailures)
                .Returns(GroupClause.Undefined);
            stepMock.Setup(step => step.ParallelCount)
                .Returns<uint?>(null);

            var registry = new Mock<IRegistry>(MockBehavior.Strict);
            registry.Setup(reg => reg.GetStartupProcessors())
                .Returns(new List<IStartupProcessor>() { processor });
            registry.Setup(reg => reg.GetStep(It.IsAny<string>()))
                .Returns<string>(n => stepMock.Object);

            var logger = new Mock<ILogger<Engine>>(MockBehavior.Loose);
            Engine engine = new Engine(logger.Object, registry.Object);

            // Act & Verify
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                engine.ProcessStartupActionsAsync());
        }


        [TestMethod]
        [TestCategory("Functional")]
        public async Task ProcessStartupActionsAsync__Asynchronous_ExecutesCorrectly_WhenStepsAreSuccessful()
        {
            // Arrange
            string stepName = "step";

            DateTime start = DateTime.Now;

            IStartupProcessor processor = defaultStartupProcessor;
            processor.Steps = new List<string> { stepName, stepName };
            processor.Asynchronous = true;

            Mock<IStep> stepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.Setup(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.Success);
            stepMock.Setup(step => step.FailOnParallelFailures)
                .Returns(GroupClause.Undefined);
            stepMock.Setup(step => step.ParallelCount)
                .Returns<uint?>(null);

            var registry = new Mock<IRegistry>(MockBehavior.Strict);
            registry.Setup(reg => reg.GetStartupProcessors())
                .Returns(new List<IStartupProcessor>() { processor });
            registry.Setup(reg => reg.GetStep(It.IsAny<string>()))
                .Returns<string>(n => stepMock.Object);

            var logger = new Mock<ILogger<Engine>>(MockBehavior.Loose);
            Engine engine = new Engine(logger.Object, registry.Object);

            // Act
            await engine.ProcessStartupActionsAsync().ConfigureAwait(false);

            // Verify
            Assert.IsTrue(true);
        }


        [TestMethod]
        [TestCategory("Functional")]
        public async Task ProcessStartupActionsAsync_Asynchronous_Logs_WhenStepsFail()
        {
            // Arrange
            string stepName = "step";

            DateTime start = DateTime.Now;

            IStartupProcessor processor = defaultStartupProcessor;
            processor.Steps = new List<string> { stepName, stepName };
            processor.Asynchronous = true;

            Mock<IStep> stepMock = new Mock<IStep>(MockBehavior.Strict);
            stepMock.Setup(step => step.ExecuteAsync())
                .ReturnsAsync(ExecutionStatus.Fail);
            stepMock.Setup(step => step.FailOnParallelFailures)
                .Returns(GroupClause.Undefined);
            stepMock.Setup(step => step.ParallelCount)
                .Returns<uint?>(null);

            var registry = new Mock<IRegistry>(MockBehavior.Strict);
            registry.Setup(reg => reg.GetStartupProcessors())
                .Returns(new List<IStartupProcessor>() { processor });
            registry.Setup(reg => reg.GetStep(It.IsAny<string>()))
                .Returns<string>(n => stepMock.Object);

            var logger = new Mock<ILogger<Engine>>(MockBehavior.Loose);
            Engine engine = new Engine(logger.Object, registry.Object);

            // Act
            await engine.ProcessStartupActionsAsync().ConfigureAwait(false);
            Thread.Sleep(20);

            // Verify
            logger.Verify(l => l.LogCritical(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }


        private readonly StartupProcessor defaultStartupProcessor = new StartupProcessor
        {
            Steps = new List<string>(),
            Asynchronous = false
        };

        private readonly RequestProcessor defaultRequestProcessor = new RequestProcessor
        {
            Steps = new List<string>(),
            ErrorPayloadSize = 0,
            SuccessPayloadSize = 0,
            IngressLatencyMilliseconds = 0
        };
    }
}
