using CoreService.Simulation.Steps;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreService.Test.Simulation.Steps
{
    [TestClass]
    public class LoadStepUnitTests
    {
        [TestMethod]
        public void Deserialization_ValidData_CreatesValidInstance()
        {
            LoadStep step = JsonConvert.DeserializeObject<LoadStep>("{ time : 10, percent : 20 }");

            Assert.AreEqual(20, step.CpuPercentage);
            Assert.AreEqual(10, step.TimeInSeconds);
        }


        [TestMethod]
        public void Deserialization_InvalidData_Throws()
        {
            Assert.ThrowsException<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<LoadStep>("{ }"));
        }


        [TestMethod]
        public async Task ExecuteAsync_InvalidPercent_Throws()
        {
            var step = new LoadStep()
            { TimeInSeconds = 15, CpuPercentage = -100 };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_InvalidTime_Throws()
        {
            var step = new LoadStep()
            { TimeInSeconds = -15, CpuPercentage = -50 };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_ValidState_ExecutesCorrectly()
        {
            var start = DateTime.UtcNow;            
            var step = new LoadStep()
            { TimeInSeconds = 1, CpuPercentage = 10 };

            await step.ExecuteAsync();
            var timeSpan = DateTime.UtcNow.Subtract(start);

            Assert.IsTrue(timeSpan.TotalSeconds >= 1.0d);

        }
    }
}
