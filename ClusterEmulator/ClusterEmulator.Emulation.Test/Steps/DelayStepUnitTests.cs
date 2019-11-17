using ClusterEmulator.Emulation.Steps;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace ClusterEmulator.Emulation.Test.Steps
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
            IStep step = new DelayStep()
            { Time = 0.2d };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_InvalidTime_Throws()
        {
            IStep step = new DelayStep()
            { Time = -2.0d };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step = step.AsTypeModel(logger.Object);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_TimeOnePointFive_ReturnsSuccess()
        {
            IStep step = new DelayStep()
            { Time = 1.5d };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step = step.AsTypeModel(logger.Object);

            ExecutionStatus status = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.Success, status);
        }


        [TestMethod]
        public async Task ExecuteAsync_TimeZero_ReturnsSuccess()
        {
            IStep step = new DelayStep()
            { Time = 0.0d };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step = step.AsTypeModel(logger.Object);

            ExecutionStatus status = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.Success, status);
        }
    }
}