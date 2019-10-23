using ClusterEmulator.Service.Simulation.Core;
using ClusterEmulator.Service.Simulation.Processors;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace ClusterEmulator.Service.Simulation.Test.Processors
{
    [TestClass]
    public class ProcessorFactroyUnitTests
    {
        [TestMethod]
        public void Constructor_WithNullLogger_ThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new ConfigFactory<RequestProcessor>(null), "Constructor should throw.");
        }


        [TestMethod]
        public void Create_WithNullName_ThrowsException()
        {
            var logger = new Mock<ILogger<ConfigFactory<RequestProcessor>>>(MockBehavior.Loose);
            var factory = new ConfigFactory<RequestProcessor>(logger.Object);

            Assert.ThrowsException<ArgumentException>(
                () => factory.Create(null), "Create should throw.");
        }


        [TestMethod]
        public void Create_WithValidSetting_ReturnsProcessor()
        {
            HashSet<string> steps = new HashSet<string>{ "A" };
            string setting = "{ errorSize : 100, latency : 42, steps : [ 'A' ], successSize : 20 }";
            var logger = new Mock<ILogger<ConfigFactory<RequestProcessor>>>(MockBehavior.Loose);
            var factory = new ConfigFactory<RequestProcessor>(logger.Object);

            IRequestProcessor processor = factory.Create(setting);

            Assert.IsNotNull(processor, "Processor should not be null");
            Assert.AreEqual(100, processor.ErrorPayloadSize, "Error size should be set correctly");
            Assert.AreEqual(42, processor.IngressLatencyMilliseconds, "Latency should be set correctly");
            Assert.AreEqual(steps.Count, processor.Steps.Count, "Steps should have the correct number of entries");
            Assert.IsTrue(steps.SetEquals(processor.Steps), "Steps should be set correctly");
            Assert.AreEqual(20, processor.SuccessPayloadSize, "Success size should be set correctly");
            Assert.AreEqual(50, processor.ErrorPayload.Error.Length, "Error payload should be the correct size");
            var resultList = processor.SuccessPayload.Result as List<string>;
            Assert.IsNotNull(resultList);
            Assert.AreEqual(1, resultList.Count, "Success result should be the correct length");
            Assert.AreEqual(10, resultList[0].Length, "Success payload should be the correct size");
        }


        [TestMethod]
        public void Create_WithNonJsonData_ReturnsNull()
        {
            string setting = "???";
            var logger = new Mock<ILogger<ConfigFactory<RequestProcessor>>>(MockBehavior.Loose);
            var factory = new ConfigFactory<RequestProcessor>(logger.Object);

            IRequestProcessor processor = factory.Create(setting);

            Assert.IsNull(processor, "Processor should be null");
        }


        [TestMethod]
        public void Create_WithEmptyJson_ReturnsNull()
        {
            string setting = "{ }";
            var logger = new Mock<ILogger<ConfigFactory<RequestProcessor>>>(MockBehavior.Loose);
            var factory = new ConfigFactory<RequestProcessor>(logger.Object);

            IRequestProcessor processor = factory.Create(setting);

            Assert.IsNull(processor, "Processor should be null");
        }
    }
}
