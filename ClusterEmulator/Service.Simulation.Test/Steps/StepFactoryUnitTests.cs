using ClusterEmulator.Service.Simulation.Core;
using ClusterEmulator.Service.Simulation.Steps;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace ClusterEmulator.Service.Simulation.Test.Steps
{
    [TestClass]
    public class StepFactoryUnitTests
    {
        private readonly Func<IStep, ILogger, IStep> Converter = new Func<IStep, ILogger, IStep>((step, log) =>
        {
            step.InitializeLogger(log);
            return step;
        });


        [TestMethod]
        public void Constructor_WithNullLogger_ThrowsException()
        {
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            Assert.ThrowsException<ArgumentNullException>(
                () => new NestedConfigFactory<IStep, IStep>(null, loggerFactory.Object, Converter),
                "Constructor should throw.");
        }


        [TestMethod]
        public void Constructor_WithNullLoggerFactory_ThrowsException()
        {
            var logger = new Mock<ILogger<NestedConfigFactory<IStep, IStep>>>(MockBehavior.Strict);

            Assert.ThrowsException<ArgumentNullException>(
                () => new NestedConfigFactory<IStep, IStep>(logger.Object, null, Converter),
                "Constructor should throw.");
        }


        [TestMethod]
        public void Constructor_WithNullConverter_Succeeds()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var logger = new Mock<ILogger<NestedConfigFactory<IStep, IStep>>>(MockBehavior.Strict);

            // Act
            var stepFactory = new NestedConfigFactory<IStep, IStep>(logger.Object, loggerFactory.Object, null);

            // Verify
            Assert.IsNotNull(stepFactory);
        }


        [TestMethod]
        public void Create_WithNullName_ThrowsException()
        {
            var logger = new Mock<ILogger<NestedConfigFactory<IStep, IStep>>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var factory = new NestedConfigFactory<IStep, IStep>(logger.Object, loggerFactory.Object, Converter);

            Assert.ThrowsException<ArgumentException>(
                () => factory.Create(null), "Create should throw.");
        }


        [TestMethod]
        public void Create_WithNonJsonData_ReturnsNull()
        {
            string setting = "???";
            var logger = new Mock<ILogger<NestedConfigFactory<IStep, IStep>>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var factory = new NestedConfigFactory<IStep, IStep>(logger.Object, loggerFactory.Object, Converter);

            IStep step = factory.Create(setting);

            Assert.IsNull(step, "Step should be null");
        }


        [TestMethod]
        public void Create_WithErronousSetting_ReturnsNull()
        {
            string setting = "{ }";
            var logger = new Mock<ILogger<NestedConfigFactory<IStep, IStep>>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var factory = new NestedConfigFactory<IStep, IStep>(logger.Object, loggerFactory.Object, Converter);

            IStep step = factory.Create(setting);

            Assert.IsNull(step, "Step should be null");
        }


        [TestMethod]
        public void Create_WithUnknownType_ThrowsException()
        {
            string setting = "{ type : 'FrontStep', value : { time : 10, percent : 20 } }";
            var logger = new Mock<ILogger<NestedConfigFactory<IStep, IStep>>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var factory = new NestedConfigFactory<IStep, IStep>(logger.Object, loggerFactory.Object, Converter);

            Assert.ThrowsException<InvalidOperationException>(
                () => factory.Create(setting), "Create should throw");
        }


        [TestMethod]
        public void Create_WithInvalidStep_ReturnsNull()
        {
            string setting = "{ type : 'LoadStep', value : {  } }";
            var logger = new Mock<ILogger<NestedConfigFactory<IStep, IStep>>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var factory = new NestedConfigFactory<IStep, IStep>(logger.Object, loggerFactory.Object, Converter);

            IStep step = factory.Create(setting);

            Assert.IsNull(step, "Step should be null");
        }


        [TestMethod]
        public void Create_WithMissingStepValue_Throws()
        {
            string setting = "{ type : 'LoadStep' }";
            var logger = new Mock<ILogger<NestedConfigFactory<IStep, IStep>>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var factory = new NestedConfigFactory<IStep, IStep>(logger.Object, loggerFactory.Object, Converter);

            Assert.ThrowsException<InvalidOperationException>(
                () => factory.Create(setting), "Create should throw");
        }


        [TestMethod]
        public void Create_WithValidSetting_ReturnsStep()
        {
            string setting = "{ type : 'LoadStep', value : { bytes : 2, time : 10, percent : 20 } }";
            var logger = new Mock<ILogger<NestedConfigFactory<IStep, IStep>>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            // Mock string implementation, called by extenstion method that takes Type
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            var factory = new NestedConfigFactory<IStep, IStep>(logger.Object, loggerFactory.Object, Converter);

            IStep step = factory.Create(setting);

            Assert.IsNotNull(step, "Step should not be null");
            Assert.IsInstanceOfType(step, typeof(LoadStep), "Step should be a LoadStep instance");
            LoadStep loadStep = step as LoadStep;
            Assert.AreEqual(10, loadStep.TimeInSeconds, "Time should be set correctly");
            Assert.AreEqual(20, loadStep.CpuPercentage, "Percentage should be set correctly");
        }
    }
}
