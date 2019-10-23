using ClusterEmulator.Service.Simulation.Steps;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace ClusterEmulator.Service.Simulation.Test.Steps
{
    [TestClass]
    public class ErrorStepUnitTests
    {
        [TestMethod]
        public void Deserialization_ValidData_CreatesValidInstance()
        {
            ErrorStep step = JsonConvert.DeserializeObject<ErrorStep>("{ probability: 0.5 }");

            Assert.AreEqual(0.5d, step.Probability, 0.0001d);
        }


        [TestMethod]
        public void Deserialization_InvalidData_Throws()
        {
            Assert.ThrowsException<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<ErrorStep>("{ }"));
        }


        [TestMethod]
        public async Task ExecuteAsync_LoggerNotInitialized_Throws()
        {
            IStep step = new ErrorStep()
            { Probability = 0.5d };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_InvalidProbability_Throws()
        {
            IStep step = new ErrorStep()
            { Probability = 2.0d };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step = step.AsTypeModel(logger.Object);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_ProbabilityOne_ReturnsFail()
        {
            IStep step = new ErrorStep()
            { Probability = 1.0d };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step = step.AsTypeModel(logger.Object);

            ExecutionStatus status = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.SimulatedFail, status);
        }


        [TestMethod]
        public async Task ExecuteAsync_ProbabilityZero_ReturnsSuccess()
        {
            IStep step = new ErrorStep()
            { Probability = 0.0d };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step = step.AsTypeModel(logger.Object);

            ExecutionStatus status = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.Success, status);
        }
    }
}