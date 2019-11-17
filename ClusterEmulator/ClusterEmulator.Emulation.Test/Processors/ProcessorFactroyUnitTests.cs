using ClusterEmulator.Emulation.Core;
using ClusterEmulator.Emulation.Processors;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace ClusterEmulator.Emulation.Test.Processors
{
    [TestClass]
    public class ProcessorFactroyUnitTests
    {
        [TestMethod]
        public void Constructor_WithNullLogger_ThrowsException()
        {
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            Assert.ThrowsException<ArgumentNullException>(
                () => new NestedConfigFactory<IProcessor, IProcessor>(null, loggerFactory.Object));
        }


        [TestMethod]
        public void Constructor_WithNullLoggerFactory_ThrowsException()
        {
            var logger = new Mock<ILogger<NestedConfigFactory<IProcessor, IProcessor>>>(MockBehavior.Strict);

            Assert.ThrowsException<ArgumentNullException>(
                () => new NestedConfigFactory<IProcessor, IProcessor>(logger.Object, null));
        }


        [TestMethod]
        public void Create_WithNullName_ThrowsException()
        {
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Loose);
            var logger = new Mock<ILogger<NestedConfigFactory<IProcessor, IProcessor>>>(MockBehavior.Loose);
            var factory = new NestedConfigFactory<IProcessor, IProcessor>(logger.Object, loggerFactory.Object);

            Assert.ThrowsException<ArgumentException>(
                () => factory.Create(null));
        }


        [TestMethod]
        public void Create_WithValidSetting_ReturnsProcessor()
        {
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Loose);
            HashSet<string> steps = new HashSet<string>{ "A" };
            string setting = "{ type : 'RequestProcessor', value : { errorSize : 100, latency : 42, steps : [ 'A' ], successSize : 20 }}";
            var logger = new Mock<ILogger<NestedConfigFactory<IProcessor, IProcessor>>>(MockBehavior.Loose);
            var factory = new NestedConfigFactory<IProcessor, IProcessor>(logger.Object, loggerFactory.Object);

            IProcessor processor = factory.Create(setting);
            Assert.IsNotNull(processor);
            IRequestProcessor reqProcessor = processor as IRequestProcessor;
            Assert.IsNotNull(reqProcessor, "Processor should not be null");
            Assert.AreEqual(100, reqProcessor.ErrorPayloadSize, "Error size should be set correctly");
            Assert.AreEqual(42, reqProcessor.IngressLatencyMilliseconds, "Latency should be set correctly");
            Assert.AreEqual(steps.Count, reqProcessor.Steps.Count, "Steps should have the correct number of entries");
            Assert.IsTrue(steps.SetEquals(reqProcessor.Steps), "Steps should be set correctly");
            Assert.AreEqual(20, reqProcessor.SuccessPayloadSize, "Success size should be set correctly");
            Assert.AreEqual(50, reqProcessor.ErrorPayload.Error.Length, "Error payload should be the correct size");
            var resultList = reqProcessor.SuccessPayload.Result as List<string>;
            Assert.IsNotNull(resultList);
            Assert.AreEqual(1, resultList.Count, "Success result should be the correct length");
            Assert.AreEqual(10, resultList[0].Length, "Success payload should be the correct size");
        }


        [TestMethod]
        public void Create_WithNonJsonData_ReturnsNull()
        {
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Loose);
            string setting = "???";
            var logger = new Mock<ILogger<NestedConfigFactory<IProcessor, IProcessor>>>(MockBehavior.Loose);
            var factory = new NestedConfigFactory<IProcessor, IProcessor>(logger.Object, loggerFactory.Object);

            IProcessor processor = factory.Create(setting);
            Assert.IsNull(processor, "Processor should be null");
        }


        [TestMethod]
        public void Create_WithEmptyJson_ReturnsNull()
        {
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Loose);
            string setting = "{ }";
            var logger = new Mock<ILogger<NestedConfigFactory<IProcessor, IProcessor>>>(MockBehavior.Loose);
            var factory = new NestedConfigFactory<IProcessor, IProcessor>(logger.Object, loggerFactory.Object);

            IProcessor processor = factory.Create(setting);
            Assert.IsNull(processor);
        }


        [TestMethod]
        public void Create_WithEmptyValueJson_ReturnsNull()
        {
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Loose);
            string setting = "{ type : 'RequestProcessor', value : {} }";
            var logger = new Mock<ILogger<NestedConfigFactory<IProcessor, IProcessor>>>(MockBehavior.Loose);
            var factory = new NestedConfigFactory<IProcessor, IProcessor>(logger.Object, loggerFactory.Object);

            IProcessor processor = factory.Create(setting);
            Assert.IsNull(processor);
        }


        [TestMethod]
        public void Create_WithMissingValueJson_Throws()
        {
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Loose);
            string setting = "{ type : 'RequestProcessor' }";
            var logger = new Mock<ILogger<NestedConfigFactory<IProcessor, IProcessor>>>(MockBehavior.Loose);
            var factory = new NestedConfigFactory<IProcessor, IProcessor>(logger.Object, loggerFactory.Object);

            Assert.ThrowsException<InvalidOperationException>(
                () => factory.Create(setting));
        }
    }
}
