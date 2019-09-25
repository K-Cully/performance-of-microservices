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
    public class LoadStepUnitTests
    {
        [TestMethod]
        public void Deserialization_ValidData_CreatesValidInstance()
        {
            ulong expectedBytes = 0;
            LoadStep step = JsonConvert.DeserializeObject<LoadStep>("{ bytes: 0, time : 10, percent : 20 }");

            Assert.AreEqual(20, step.CpuPercentage);
            Assert.AreEqual(10, step.TimeInSeconds);
            Assert.AreEqual(expectedBytes, step.MemoryInBytes);
        }


        [TestMethod]
        public void Deserialization_InvalidData_Throws()
        {
            Assert.ThrowsException<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<LoadStep>("{ }"));
        }


        [TestMethod]
        public async Task ExecuteAsync_LoggerNotInitialized_Throws()
        {
            var step = new LoadStep()
            { MemoryInBytes = 2, TimeInSeconds = 15, CpuPercentage = 2 };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_InvalidPercent_Throws()
        {
            var step = new LoadStep()
            { MemoryInBytes = 2, TimeInSeconds = 15, CpuPercentage = -100 };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step.InitializeLogger(logger.Object);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_InvalidTime_Throws()
        {
            var step = new LoadStep()
            { MemoryInBytes = 5, TimeInSeconds = -15, CpuPercentage = 5 };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step.InitializeLogger(logger.Object);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_ValidState_ExecutesCorrectly()
        {
            var start = DateTime.UtcNow;
            var step = new LoadStep()
            { MemoryInBytes = 10, TimeInSeconds = 1, CpuPercentage = 10 };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step.InitializeLogger(logger.Object);

            ExecutionStatus status = await step.ExecuteAsync();
            var timeSpan = DateTime.UtcNow.Subtract(start);

            Assert.IsTrue(timeSpan.TotalSeconds >= 1.0d);
            Assert.AreEqual(ExecutionStatus.Success, status);
        }


        [TestMethod]
        public async Task ExecuteAsync_ValidBytesOne_ExecutesCorrectly()
        {
            var start = DateTime.UtcNow;
            var step = new LoadStep()
            { MemoryInBytes = 1, TimeInSeconds = 1, CpuPercentage = 1 };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step.InitializeLogger(logger.Object);

            ExecutionStatus status = await step.ExecuteAsync();
            var timeSpan = DateTime.UtcNow.Subtract(start);

            Assert.IsTrue(timeSpan.TotalSeconds >= 1.0d);
            Assert.AreEqual(ExecutionStatus.Success, status);
        }


        [TestMethod]
        public async Task ExecuteAsync_ValidBytesMax_Throws()
        {
            var step = new LoadStep()
            { MemoryInBytes = ulong.MaxValue, TimeInSeconds = 15, CpuPercentage = 50 };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step.InitializeLogger(logger.Object);

            await Assert.ThrowsExceptionAsync<OutOfMemoryException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public void InitializeLogger_NullLogger_ThrowsException()
        {
            var step = new LoadStep();

            Assert.ThrowsException<ArgumentNullException>(
                () => step.InitializeLogger(null));
        }
    }
}
