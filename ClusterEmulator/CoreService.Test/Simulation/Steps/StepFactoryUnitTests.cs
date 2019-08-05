using CoreService.Simulation.Steps;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CoreService.Test.Simulation.Steps
{
    [TestClass]
    public class StepFactoryUnitTests
    {
        [TestMethod]
        public void Create_WithNullName_ThrowsException()
        {
            var factory = new StepFactory();

            Assert.ThrowsException<ArgumentException>(
                () => factory.Create(null), "Create should throw.");
        }


        [TestMethod]
        public void Create_WithNonJsonData_ReturnsNull()
        {
            string setting = "???";
            var factory = new StepFactory();

            IStep step = factory.Create(setting);

            Assert.IsNull(step, "Step should be null");
        }


        [TestMethod]
        public void Create_WithErronousSetting_ReturnsNull()
        {
            string setting = "{ }";
            var factory = new StepFactory();

            IStep step = factory.Create(setting);

            Assert.IsNull(step, "Step should be null");
        }


        [TestMethod]
        public void Create_WithUnknownType_ThrowsException()
        {
            string setting = "{ type : 'FrontStep', step : { time : 10, percent : 20 } }";
            var factory = new StepFactory();

            Assert.ThrowsException<InvalidOperationException>(
                () => factory.Create(setting), "Create should throw");
        }


        [TestMethod]
        public void Create_WithInValidSetting_ReturnsNull()
        {
            string setting = "{ type : 'LoadStep', step : {  } }";
            var factory = new StepFactory();

            IStep step = factory.Create(setting);

            Assert.IsNull(step, "Step should be null");
        }


        [TestMethod]
        public void Create_WithValidSetting_ReturnsStep()
        {
            string setting = "{ type : 'LoadStep', step : { bytes : 2, time : 10, percent : 20 } }";
            var factory = new StepFactory();

            IStep step = factory.Create(setting);

            Assert.IsNotNull(step, "Step should not be null");
            Assert.IsInstanceOfType(step, typeof(LoadStep), "Step should be a LoadStep instance");
            LoadStep loadStep = step as LoadStep;
            Assert.AreEqual(10, loadStep.TimeInSeconds, "Time should be set correctly");
            Assert.AreEqual(20, loadStep.CpuPercentage, "Percentage should be set correctly");
        }
    }
}
