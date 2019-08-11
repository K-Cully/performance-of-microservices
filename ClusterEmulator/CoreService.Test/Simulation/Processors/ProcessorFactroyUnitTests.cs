using CoreService.Simulation.Core;
using CoreService.Simulation.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CoreService.Test.Simulation.Processors
{
    [TestClass]
    public class ProcessorFactroyUnitTests
    {
        [TestMethod]
        public void Create_WithNullName_ThrowsException()
        {
            var factory = new ConfigFactory<Processor>();

            Assert.ThrowsException<ArgumentException>(
                () => factory.Create(null), "Create should throw.");
        }


        [TestMethod]
        public void Create_WithValidSetting_ReturnsProcessor()
        {
            HashSet<string> steps = new HashSet<string>{ "A" };
            string setting = "{ errorSize : 100, latency : 42, steps : [ 'A' ], successSize : 20 }";
            var factory = new ConfigFactory<Processor>();

            IProcessor processor = factory.Create(setting);

            Assert.IsNotNull(processor, "Processor should not be null");
            Assert.AreEqual(100, processor.ErrorPayloadSize, "Error size should be set correctly");
            Assert.AreEqual(42, processor.IngressLatencyMilliseconds, "Latency should be set correctly");
            Assert.AreEqual(steps.Count, processor.Steps.Count, "Steps should have the correct number of entries");
            Assert.IsTrue(steps.SetEquals(processor.Steps), "Steps should be set correctly");
            Assert.AreEqual(20, processor.SuccessPayloadSize, "Success size should be set correctly");
            Assert.AreEqual(100, processor.ErrorPayload.Error.Length, "Error payload should be the correct size");
            Assert.AreEqual(20, processor.SuccessPayload.Result.Length, "Success payload should be the correct size");
        }


        [TestMethod]
        public void Create_WithNonJsonData_ReturnsNull()
        {
            string setting = "???";
            var factory = new ConfigFactory<Processor>();

            IProcessor processor = factory.Create(setting);

            Assert.IsNull(processor, "Processor should be null");
        }


        [TestMethod]
        public void Create_WithEmptyJson_ReturnsNull()
        {
            string setting = "{ }";
            var factory = new ConfigFactory<Processor>();

            IProcessor processor = factory.Create(setting);

            Assert.IsNull(processor, "Processor should be null");
        }
    }
}
