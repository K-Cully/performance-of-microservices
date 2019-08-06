using CoreService.Simulation.Core;
using CoreService.Simulation.HttpClient;
using CoreService.Simulation.Processors;
using CoreService.Simulation.Steps;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Polly;
using Polly.Registry;
using ServiceFabric.Mocks;
using System;
using System.Fabric.Description;

using ConfigurationSectionCollection = ServiceFabric.Mocks.MockConfigurationPackage.ConfigurationSectionCollection;

namespace CoreService.Test.Simulation.Core
{
    [TestClass]
    public class RegistryUnitTests
    {
        [TestMethod]
        public void Constructor_Throws_WhenConfigurationSettingsIsNull()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(null, new StepFactory(), new ProcessorFactory(), new PolicyFactory()));
        }


        [TestMethod]
        public void Constructor_Throws_WhenStepFactoryIsNull()
        {
            var configurations = new ConfigurationSectionCollection();
            var settings = MockConfigurationPackage.CreateConfigurationSettings(configurations);

            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(settings, null, new ProcessorFactory(), new PolicyFactory()));
        }


        [TestMethod]
        public void Constructor_Throws_WhenProcessorFactoryIsNull()
        {
            var configurations = new ConfigurationSectionCollection();
            var settings = MockConfigurationPackage.CreateConfigurationSettings(configurations);

            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(settings, new StepFactory(), null, new PolicyFactory()));
        }


        [TestMethod]
        public void Constructor_Throws_WhenPolicyFactoryIsNull()
        {
            var configurations = new ConfigurationSectionCollection();
            var settings = MockConfigurationPackage.CreateConfigurationSettings(configurations);

            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(settings, new StepFactory(), new ProcessorFactory(), null));
        }


        [TestMethod]
        public void Constructor_Throws_WhenSettingsSectionsAreMissing()
        {
            // Create SF.Mock settings
            var configurations = new ConfigurationSectionCollection();
            var settings = MockConfigurationPackage.CreateConfigurationSettings(configurations);

            // Create Moq proxy instances
            Mock<IStepFactory> stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            Mock<IProcessorFactory> processorFactory = new Mock<IProcessorFactory>(MockBehavior.Strict);
            Mock<IPolicyFactory> policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);

            // Act
            Assert.ThrowsException<InvalidOperationException>(()
                => new Registry(settings, stepFactory.Object, processorFactory.Object, policyFactory.Object));

            // Verify
            stepFactory.Verify(f => f.Create(It.IsAny<string>()), Times.Never);
            processorFactory.Verify(f => f.Create(It.IsAny<string>()), Times.Never);
            policyFactory.Verify(f => f.Create(It.IsAny<string>()), Times.Never);
        }


        [TestMethod]
        public void Constructor_CallsFactoriesCorrectly_WhenSettingsArePresent()
        {
            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq proxy instances
            Mock<IStepFactory> stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            Mock<IProcessorFactory> processorFactory = new Mock<IProcessorFactory>(MockBehavior.Strict);
            Mock<IPolicyFactory> policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);

            // Act
            Registry registry = new Registry(settings, stepFactory.Object, processorFactory.Object, policyFactory.Object);
            var policyRegistry = registry.PolicyRegistry;

            // Verify
            stepFactory.Verify(f => f.Create(It.IsAny<string>()), Times.Exactly(2));
            processorFactory.Verify(f => f.Create(It.IsAny<string>()), Times.Exactly(1));
            policyFactory.Verify(f => f.Create(It.IsAny<string>()), Times.Exactly(3));
            Assert.IsNotNull(policyRegistry, "Policy regsistry should be initialized");
            Assert.AreEqual(3, policyRegistry.Count);
        }


        [TestMethod]
        public void GetProcessor_ReturnsCorrectValue_WhenRegistered()
        {
            string processorName = "Bob";

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq proxy instances
            Mock<IProcessor> mockProcessor = new Mock<IProcessor>(MockBehavior.Strict);
            Mock<IStepFactory> stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            Mock<IPolicyFactory> policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            Mock<IProcessorFactory> processorFactory = new Mock<IProcessorFactory>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>((s) => mockProcessor.Object);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);

            // Act
            Registry registry = new Registry(settings, stepFactory.Object, processorFactory.Object, policyFactory.Object);
            IProcessor processor = registry.GetProcessor(processorName);

            // Verify
            Assert.IsNotNull(processor, "GetProcessor should return the registered value");
        }


        [TestMethod]
        public void GetProcessor_Throws_WhenRegisteredValueIsNull()
        {
            string processorName = "Bob";

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq proxy instances
            Mock<IStepFactory> stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            Mock<IProcessorFactory> processorFactory = new Mock<IProcessorFactory>(MockBehavior.Strict);
            Mock<IPolicyFactory> policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);

            // Act
            Registry registry = new Registry(settings, stepFactory.Object, processorFactory.Object, policyFactory.Object);

            // Verify
            Assert.ThrowsException<InvalidOperationException>(() => registry.GetProcessor(processorName));
        }


        [TestMethod]
        public void GetProcessor_Throws_WhenNameIsNotRegistered()
        {
            string processorName = "Xi";

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq proxy instances
            Mock<IProcessor> mockProcessor = new Mock<IProcessor>(MockBehavior.Strict);
            Mock<IStepFactory> stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            Mock<IPolicyFactory> policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            Mock<IProcessorFactory> processorFactory = new Mock<IProcessorFactory>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => mockProcessor.Object);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);

            // Act
            Registry registry = new Registry(settings, stepFactory.Object, processorFactory.Object, policyFactory.Object);

            // Verify
            Assert.ThrowsException<InvalidOperationException>(() => registry.GetProcessor(processorName));
        }


        [TestMethod]
        public void GetProcessor_Throws_WhenNameIsNull()
        {
            string processorName = null;

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq proxy instances
            Mock<IProcessor> mockProcessor = new Mock<IProcessor>(MockBehavior.Strict);
            Mock<IStepFactory> stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            Mock<IPolicyFactory> policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            Mock<IProcessorFactory> processorFactory = new Mock<IProcessorFactory>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => mockProcessor.Object);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);

            // Act
            Registry registry = new Registry(settings, stepFactory.Object, processorFactory.Object, policyFactory.Object);

            // Verify
            Assert.ThrowsException<ArgumentException>(() => registry.GetProcessor(processorName));
        }


        [TestMethod]
        public void GetStep_ReturnsCorrectValue_WhenRegistered()
        {
            string stepName = "Mary";

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq proxy instances
            Mock<IStep> stepMock = new Mock<IStep>(MockBehavior.Strict);
            Mock<IStepFactory> stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            Mock<IProcessorFactory> processorFactory = new Mock<IProcessorFactory>(MockBehavior.Strict);
            Mock<IPolicyFactory> policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => stepMock.Object);
            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);

            // Act
            Registry registry = new Registry(settings, stepFactory.Object, processorFactory.Object, policyFactory.Object);
            IStep step = registry.GetStep(stepName);

            // Verify
            Assert.IsNotNull(step, "GetStep should return the registered value");
        }


        [TestMethod]
        public void GetPolicy_ReturnsCorrectValue_WhenRegistered()
        {
            string policyName = "Max";

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq proxy instances
            Mock<Policy> policyMock = new Mock<Policy>(MockBehavior.Strict);
            Mock<IStepFactory> stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            Mock<IProcessorFactory> processorFactory = new Mock<IProcessorFactory>(MockBehavior.Strict);
            Mock<IPolicyFactory> policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => policyMock.Object);

            // Act
            Registry registry = new Registry(settings, stepFactory.Object, processorFactory.Object, policyFactory.Object);
            Policy policy = registry.GetPolicy(policyName);

            // Verify
            Assert.IsNotNull(policy, "GetPolicy should return the registered value");
        }


        // TODO: test GetPolicies and PolicyRegistry


        private ConfigurationSettings CreateDefaultSettings()
        {
            // Create configuration structures
            var configurations = new ConfigurationSectionCollection();
            var settings = MockConfigurationPackage.CreateConfigurationSettings(configurations);

            // Add sections
            var processorSection = MockConfigurationPackage.CreateConfigurationSection(Registry.ProcessorsSection);
            var stepSection = MockConfigurationPackage.CreateConfigurationSection(Registry.StepsSection);
            var policiesSection = MockConfigurationPackage.CreateConfigurationSection(Registry.PoliciesSection);
            configurations.Add(processorSection);
            configurations.Add(stepSection);
            configurations.Add(policiesSection);

            // Add processors parameters
            ConfigurationProperty processor = MockConfigurationPackage
                .CreateConfigurationSectionParameters("Bob", "");
            processorSection.Parameters.Add(processor);

            // Add step parameters
            ConfigurationProperty step1 = MockConfigurationPackage
                .CreateConfigurationSectionParameters("Frank", "");
            ConfigurationProperty step2 = MockConfigurationPackage
                .CreateConfigurationSectionParameters("Mary", "");
            stepSection.Parameters.Add(step1);
            stepSection.Parameters.Add(step2);

            // Add policy parameters
            ConfigurationProperty policy1 = MockConfigurationPackage
                .CreateConfigurationSectionParameters("Amanda", "");
            ConfigurationProperty policy2 = MockConfigurationPackage
                .CreateConfigurationSectionParameters("Max", "");
            ConfigurationProperty policy3 = MockConfigurationPackage
                .CreateConfigurationSectionParameters("Ted", "");
            policiesSection.Parameters.Add(policy1);
            policiesSection.Parameters.Add(policy2);
            policiesSection.Parameters.Add(policy3);

            return settings;
        }
    }
}
