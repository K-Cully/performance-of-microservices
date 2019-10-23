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
        public void Deserialization_RequiredData_CreatesValidInstance()
        {
            ulong expectedBytes = 0;
            LoadStep step = JsonConvert.DeserializeObject<LoadStep>("{ bytes: 0, time : 10.2, percent : 20 }");

            Assert.AreEqual(20, step.CpuPercentage);
            Assert.AreEqual(10.2d, step.TimeInSeconds, 0.0001d);
            Assert.AreEqual(expectedBytes, step.MemoryInBytes);
            Assert.AreEqual(0, step.MaxProcessors);
        }


        [TestMethod]
        public void Deserialization_AllData_CreatesValidInstance()
        {
            ulong expectedBytes = 0;
            LoadStep step = JsonConvert.DeserializeObject<LoadStep>("{ bytes: 0, time : 10, percent : 20, processors : 2 }");

            Assert.AreEqual(20, step.CpuPercentage);
            Assert.AreEqual(10.0d, step.TimeInSeconds, 0.0001d);
            Assert.AreEqual(expectedBytes, step.MemoryInBytes);
            Assert.AreEqual(2, step.MaxProcessors);
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
            IStep step = new LoadStep()
            { MemoryInBytes = 2, TimeInSeconds = 5, CpuPercentage = 2 };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_InvalidPercent_Throws()
        {
            IStep step = new LoadStep()
            { MemoryInBytes = 2, TimeInSeconds = 15.1d, CpuPercentage = -100, };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step = step.AsTypeModel(logger.Object);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_InvalidMaxProcessors_Throws()
        {
            IStep step = new LoadStep()
            { MemoryInBytes = 5, TimeInSeconds = 2.0d, CpuPercentage = 5, MaxProcessors = -1 };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step = step.AsTypeModel(logger.Object);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_ValidState_ExecutesCorrectly()
        {
            IStep step = new LoadStep()
            { MemoryInBytes = 10, TimeInSeconds = 0.0d, CpuPercentage = 10 };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step = step.AsTypeModel(logger.Object);

            ExecutionStatus status = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.Success, status);
        }


        [TestMethod]
        public async Task ExecuteAsync_ProcessorOverrideHigherThanProcessorCount_ExecutesCorrectly()
        {
            var start = DateTime.UtcNow;
            IStep step = new LoadStep()
            { MemoryInBytes = 10, TimeInSeconds = 0.2d, CpuPercentage = 10, MaxProcessors = int.MaxValue };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step = step.AsTypeModel(logger.Object);

            ExecutionStatus status = await step.ExecuteAsync();
            var timeSpan = DateTime.UtcNow.Subtract(start);

            Assert.IsTrue(timeSpan.TotalSeconds >= 0.2d);
            Assert.AreEqual(ExecutionStatus.Success, status);
        }


        [TestMethod]
        public async Task ExecuteAsync_ProcessorCountOverriden_ExecutesCorrectly()
        {
            var start = DateTime.UtcNow;
            IStep step = new LoadStep()
            { MemoryInBytes = 10, TimeInSeconds = 0.2d, CpuPercentage = 10, MaxProcessors = 1 };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step = step.AsTypeModel(logger.Object);

            ExecutionStatus status = await step.ExecuteAsync();
            var timeSpan = DateTime.UtcNow.Subtract(start);

            Assert.IsTrue(timeSpan.TotalSeconds >= 0.2d);
            Assert.AreEqual(ExecutionStatus.Success, status);
        }


        [TestMethod]
        public async Task ExecuteAsync_ValidBytesOne_ExecutesCorrectly()
        {
            var start = DateTime.UtcNow;
            IStep step = new LoadStep()
            { MemoryInBytes = 1, TimeInSeconds = 1.0d, CpuPercentage = 1 };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step = step.AsTypeModel(logger.Object);

            ExecutionStatus status = await step.ExecuteAsync();
            var timeSpan = DateTime.UtcNow.Subtract(start);

            Assert.IsTrue(timeSpan.TotalSeconds >= 1.0d);
            Assert.AreEqual(ExecutionStatus.Success, status);
        }


        [TestMethod]
        public async Task ExecuteAsync_ValidBytesMax_Throws()
        {
            IStep step = new LoadStep()
            { MemoryInBytes = ulong.MaxValue, TimeInSeconds = 2, CpuPercentage = 0 };
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            step = step.AsTypeModel(logger.Object);

            await Assert.ThrowsExceptionAsync<OutOfMemoryException>(
                () => step.ExecuteAsync());
        }
    }
}
