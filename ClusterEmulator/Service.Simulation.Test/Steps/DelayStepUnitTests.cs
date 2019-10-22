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
    public class DelayStepUnitTests
    {
        [TestMethod]
        public void Deserialization_ValidData_CreatesValidInstance()
        {
            DelayStep step = JsonConvert.DeserializeObject<DelayStep>("{ time: 0.5 }");

            Assert.AreEqual(0.5d, step.Time, 0.0001d);
        }


        [TestMethod]
        public void Deserialization_InvalidData_Throws()
        {
            Assert.ThrowsException<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<DelayStep>("{ }"));
        }


        [TestMethod]
        public async Task ExecuteAsync_LoggerNotInitialized_Throws()
        {
            var step = new DelayStep()
            { Time = 2.0d };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_InvalidTime_Throws()
        {
            var step = new DelayStep()
            { Time = -2.0d };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step.InitializeLogger(logger.Object);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_TimeTen_ReturnsSuccess()
        {
            var step = new DelayStep()
            { Time = 10.0d };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step.InitializeLogger(logger.Object);

            ExecutionStatus status = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.Success, status);
        }


        [TestMethod]
        public async Task ExecuteAsync_TimeZero_ReturnsSuccess()
        {
            var step = new DelayStep()
            { Time = 0.0d };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step.InitializeLogger(logger.Object);

            ExecutionStatus status = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.Success, status);
        }
    }
}