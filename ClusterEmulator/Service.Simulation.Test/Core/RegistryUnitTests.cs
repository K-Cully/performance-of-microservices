using CoreService.Simulation.Core;
using CoreService.Simulation.HttpClientConfiguration;
using CoreService.Simulation.Processors;
using CoreService.Simulation.Steps;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Polly;
using Polly.Wrap;
using ServiceFabric.Mocks;
using System;
using System.Collections.Generic;
using System.Fabric.Description;
using System.Net.Http;
using ConfigurationSectionCollection = ServiceFabric.Mocks.MockConfigurationPackage.ConfigurationSectionCollection;

namespace CoreService.Test.Simulation.Core
{
    [TestClass]
    public class RegistryUnitTests
    {
        [TestMethod]
        public void Constructor_Throws_WhenConfigurationSettingsIsNull()
        {
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(null, stepFactory.Object, processorFactory.Object, policyFactory.Object,
                                clientFactory.Object, logger.Object, loggerFactory.Object));
        }


        [TestMethod]
        public void Constructor_Throws_WhenStepFactoryIsNull()
        {
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            var configurations = new ConfigurationSectionCollection();
            var settings = MockConfigurationPackage.CreateConfigurationSettings(configurations);

            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(settings, null, processorFactory.Object, policyFactory.Object,
                                clientFactory.Object, logger.Object, loggerFactory.Object));
        }


        [TestMethod]
        public void Constructor_Throws_WhenProcessorFactoryIsNull()
        {
            var configurations = new ConfigurationSectionCollection();
            var settings = MockConfigurationPackage.CreateConfigurationSettings(configurations);

            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(settings, stepFactory.Object, null, policyFactory.Object,
                                clientFactory.Object, logger.Object, loggerFactory.Object));
        }


        [TestMethod]
        public void Constructor_Throws_WhenPolicyFactoryIsNull()
        {
            var configurations = new ConfigurationSectionCollection();
            var settings = MockConfigurationPackage.CreateConfigurationSettings(configurations);

            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(settings, stepFactory.Object, processorFactory.Object,
                                null, clientFactory.Object, logger.Object, loggerFactory.Object));
        }


        [TestMethod]
        public void Constructor_Throws_WhenClientFactoryIsNull()
        {
            var configurations = new ConfigurationSectionCollection();
            var settings = MockConfigurationPackage.CreateConfigurationSettings(configurations);

            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(settings, stepFactory.Object, processorFactory.Object,
                                policyFactory.Object, null, logger.Object, loggerFactory.Object));
        }


        [TestMethod]
        public void Constructor_Throws_WhenLoggerIsNull()
        {
            var configurations = new ConfigurationSectionCollection();
            var settings = MockConfigurationPackage.CreateConfigurationSettings(configurations);
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(settings, stepFactory.Object, processorFactory.Object,
                                policyFactory.Object, clientFactory.Object, null, loggerFactory.Object));
        }


        [TestMethod]
        public void Constructor_Throws_WhenLoggerFActoryIsNull()
        {
            var configurations = new ConfigurationSectionCollection();
            var settings = MockConfigurationPackage.CreateConfigurationSettings(configurations);
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);

            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(settings, stepFactory.Object, processorFactory.Object,
                                policyFactory.Object, clientFactory.Object, logger.Object, null));
        }


        [TestMethod]
        public void Constructor_Throws_WhenSettingsSectionsAreMissing()
        {
            // Create SF.Mock settings
            var configurations = new ConfigurationSectionCollection();
            var settings = MockConfigurationPackage.CreateConfigurationSettings(configurations);

            // Create Moq proxy instances
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            // Act
            Assert.ThrowsException<InvalidOperationException>(()
                => new Registry(settings, stepFactory.Object, processorFactory.Object,
                    policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object));

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
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);
            var policyRegistry = registry.PolicyRegistry;
            var clients = new List<KeyValuePair<string, ClientConfig>>(registry.Clients);

            // Verify
            stepFactory.Verify(f => f.Create(It.IsAny<string>()), Times.Exactly(2));
            processorFactory.Verify(f => f.Create(It.IsAny<string>()), Times.Exactly(1));
            policyFactory.Verify(f => f.Create(It.IsAny<string>()), Times.Exactly(3));
            clientFactory.Verify(f => f.Create(It.IsAny<string>()), Times.Exactly(1));
            Assert.IsNotNull(policyRegistry);
            Assert.AreEqual(3, policyRegistry.Count);
            Assert.IsNotNull(clients);
            Assert.AreEqual(1, clients.Count);
            Assert.AreEqual("Xi", clients[0].Key);
            Assert.AreEqual(null, clients[0].Value);
        }


        [TestMethod]
        public void GetProcessor_ReturnsCorrectValue_WhenRegistered()
        {
            string processorName = "Bob";
            Processor expectedProcessor = new Processor();

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq proxy instances
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>((s) => expectedProcessor);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);
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
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);

            // Verify
            Assert.ThrowsException<InvalidOperationException>(() => registry.GetProcessor(processorName));
        }


        [TestMethod]
        public void GetProcessor_Throws_WhenNameIsNotRegistered()
        {
            string processorName = "Xi";
            Processor expectedProcessor = new Processor();

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq proxy instances
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => expectedProcessor);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);

            // Verify
            Assert.ThrowsException<InvalidOperationException>(() => registry.GetProcessor(processorName));
        }


        [TestMethod]
        public void GetProcessor_Throws_WhenNameIsNull()
        {
            string processorName = null;
            Processor expectedProcessor = new Processor();

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq proxy instances
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => expectedProcessor);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);

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
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => stepMock.Object);
            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);
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
             var expectedPolicy = Policy.NoOpAsync<HttpResponseMessage>();

            // Create Moq proxy instances
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => expectedPolicy);
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);
            IAsyncPolicy<HttpResponseMessage> policy = registry.GetPolicy(policyName);

            // Verify
            Assert.IsNotNull(policy, "GetPolicy should return the registered value");
        }


        [TestMethod]
        public void GetClient_ReturnsCorrectValue_WhenRegistered()
        {
            string clientName = "Xi";

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();
            var expectedConfig = new ClientConfig();

            // Create Moq proxy instances
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => expectedConfig);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);
            ClientConfig config = registry.GetClient(clientName);

            // Verify
            Assert.IsNotNull(config, "GetClient should return the registered value");
        }


        [TestMethod]
        public void ConfigureHttpClients_NullFactory_Throws()
        {
            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq proxy instances
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);

            // Verify
            Assert.ThrowsException<ArgumentNullException>(
                () => registry.ConfigureHttpClients(null));
        }


        [TestMethod]
        public void ConfigureHttpClients_NoRequestSteps_DoesNothing()
        {
            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq proxy instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Verify
            registry.ConfigureHttpClients(httpClientFactory.Object);
        }


        [TestMethod]
        public void ConfigureHttpClients_EmptyPolicies_CallsConfigureForBothReuseCases()
        {
            var expectedConfig = new ClientConfig
            {
                Policies = new List<string>()
            };

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();
            
            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);

            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => expectedConfig);

            // Create Moq steps
            var requestStep1 = new Mock<IRequestStep>(MockBehavior.Strict);
            requestStep1.Setup(rs => rs.ReuseHttpMessageHandler).Returns(true);
            requestStep1.Setup(rs => rs.ClientName).Returns("WhoKnows?");
            requestStep1.Setup(rs => rs.Configure(It.IsAny<IHttpClientFactory>()));

            var step1 = requestStep1.As<IStep>();
            stepFactory.Setup(f => f.Create("Frank"))
                .Returns<string>(s => step1.Object);
 
            var requestStep2 = new Mock<IRequestStep>(MockBehavior.Strict);
            requestStep2.Setup(rs => rs.ReuseHttpMessageHandler).Returns(false);
            requestStep2.Setup(rs => rs.ClientName).Returns("Xi");
            requestStep2.Setup(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), null));

            var step2 = requestStep2.As<IStep>();
            stepFactory.Setup(f => f.Create("Mary"))
                .Returns<string>(s => step2.Object);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);
            registry.ConfigureHttpClients(httpClientFactory.Object);

            // Verify
            requestStep1.Verify(rs => rs.Configure(It.IsAny<IHttpClientFactory>()), Times.Once);
            requestStep2.Verify(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), null), Times.Once);
        }


        [TestMethod]
        public void ConfigureHttpClients_EmptyClient_Throws()
        {
            var expectedConfig = new ClientConfig
            {
                Policies = new List<string>()
            };

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);

            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => expectedConfig);

            // Create Moq steps
            var step1 = new Mock<IStep>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create("Frank"))
                .Returns<string>(null);

            var requestStep = new Mock<IRequestStep>(MockBehavior.Strict);
            requestStep.Setup(rs => rs.ReuseHttpMessageHandler).Returns(false);
            requestStep.Setup(rs => rs.ClientName).Returns(string.Empty);
            requestStep.Setup(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), null));

            var step2 = requestStep.As<IStep>();
            stepFactory.Setup(f => f.Create("Mary"))
                .Returns<string>(s => step2.Object);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);

            // Verify
            Assert.ThrowsException<ArgumentException>(
                () => registry.ConfigureHttpClients(httpClientFactory.Object));
            requestStep.Verify(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), null), Times.Never);
        }


        [TestMethod]
        public void ConfigureHttpClients_MissingClient_Throws()
        {
            var expectedConfig = new ClientConfig
            {
                Policies = new List<string>()
            };

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);

            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => expectedConfig);

            // Create Moq steps
            var step1 = new Mock<IStep>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create("Frank"))
                .Returns<string>(null);

            var requestStep = new Mock<IRequestStep>(MockBehavior.Strict);
            requestStep.Setup(rs => rs.ReuseHttpMessageHandler).Returns(false);
            requestStep.Setup(rs => rs.ClientName).Returns("MissingEntry");
            requestStep.Setup(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), null));

            var step2 = requestStep.As<IStep>();
            stepFactory.Setup(f => f.Create("Mary"))
                .Returns<string>(s => step2.Object);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);

            // Verify
            Assert.ThrowsException<InvalidOperationException>(
                () => registry.ConfigureHttpClients(httpClientFactory.Object));
            requestStep.Verify(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), null), Times.Never);
        }


        [TestMethod]
        public void ConfigureHttpClients_NullClient_Throws()
        {
            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);

            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);

            // Create Moq steps
            var step1 = new Mock<IStep>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create("Frank"))
                .Returns<string>(null);

            var requestStep = new Mock<IRequestStep>(MockBehavior.Strict);
            requestStep.Setup(rs => rs.ReuseHttpMessageHandler).Returns(false);
            requestStep.Setup(rs => rs.ClientName).Returns("Xi");
            requestStep.Setup(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), null));

            var step2 = requestStep.As<IStep>();
            stepFactory.Setup(f => f.Create("Mary"))
                .Returns<string>(s => step2.Object);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);

            // Verify
            Assert.ThrowsException<InvalidOperationException>(
                () => registry.ConfigureHttpClients(httpClientFactory.Object));
            requestStep.Verify(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), null), Times.Never);
        }


        [TestMethod]
        public void ConfigureHttpClients_TwoPolicies_CallsConfigureWithPolicyWrap()
        {
            var expectedConfig = new ClientConfig
            {
                Policies = new List<string> { "Amanda", "Ted" }
            };

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);

            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => Policy.NoOpAsync<HttpResponseMessage>());
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => expectedConfig);

            // Create Moq steps
            var step1 = new Mock<IStep>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create("Frank"))
                .Returns<string>(null);

            var requestStep = new Mock<IRequestStep>(MockBehavior.Strict);
            requestStep.Setup(rs => rs.ReuseHttpMessageHandler).Returns(false);
            requestStep.Setup(rs => rs.ClientName).Returns("Xi");
            requestStep.Setup(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), It.IsAny<AsyncPolicyWrap<HttpResponseMessage>>()));

            var step2 = requestStep.As<IStep>();
            stepFactory.Setup(f => f.Create("Mary"))
                .Returns<string>(s => step2.Object);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);
            registry.ConfigureHttpClients(httpClientFactory.Object);

            // Verify
            requestStep.Verify(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), It.IsAny<AsyncPolicyWrap<HttpResponseMessage>>()), Times.Once);
        }


        [TestMethod]
        public void ConfigureHttpClients_OnePolicy_CallsConfigureWithPolicy()
        {
            var expectedConfig = new ClientConfig
            {
                Policies = new List<string> { "Amanda" }
            };

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);

            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => Policy.NoOpAsync<HttpResponseMessage>());
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => expectedConfig);

            // Create Moq steps
            var step1 = new Mock<IStep>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create("Frank"))
                .Returns<string>(null);

            var requestStep = new Mock<IRequestStep>(MockBehavior.Strict);
            requestStep.Setup(rs => rs.ReuseHttpMessageHandler).Returns(false);
            requestStep.Setup(rs => rs.ClientName).Returns("Xi");
            requestStep.Setup(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), It.IsAny<AsyncPolicy<HttpResponseMessage>>()));
            requestStep.Setup(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), It.IsAny<AsyncPolicyWrap<HttpResponseMessage>>()));

            var step2 = requestStep.As<IStep>();
            stepFactory.Setup(f => f.Create("Mary"))
                .Returns<string>(s => step2.Object);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);
            registry.ConfigureHttpClients(httpClientFactory.Object);

            // Verify
            requestStep.Verify(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), It.IsAny<AsyncPolicy<HttpResponseMessage>>()), Times.Once);
            requestStep.Verify(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), It.IsAny<AsyncPolicyWrap<HttpResponseMessage>>()), Times.Never);
        }


        [TestMethod]
        public void ConfigureHttpClients_PoliciesNull_Throws()
        {
            var expectedConfig = new ClientConfig
            {
                Policies = null
            };

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);

            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => Policy.NoOpAsync<HttpResponseMessage>());
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => expectedConfig);

            // Create Moq steps
            var step1 = new Mock<IStep>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create("Frank"))
                .Returns<string>(null);

            var requestStep = new Mock<IRequestStep>(MockBehavior.Strict);
            requestStep.Setup(rs => rs.ReuseHttpMessageHandler).Returns(false);
            requestStep.Setup(rs => rs.ClientName).Returns("Xi");
            requestStep.Setup(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), It.IsAny<AsyncPolicy<HttpResponseMessage>>()));

            var step2 = requestStep.As<IStep>();
            stepFactory.Setup(f => f.Create("Mary"))
                .Returns<string>(s => step2.Object);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);

            // Verify
            Assert.ThrowsException<ArgumentNullException>(
                () => registry.ConfigureHttpClients(httpClientFactory.Object));
            requestStep.Verify(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), It.IsAny<AsyncPolicy<HttpResponseMessage>>()), Times.Never);
        }


        [TestMethod]
        public void ConfigureHttpClients_PolicyNull_Throws()
        {
            var expectedConfig = new ClientConfig
            {
                Policies = new List<string> { null }
            };

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);

            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => Policy.NoOpAsync<HttpResponseMessage>());
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => expectedConfig);

            // Create Moq steps
            var step1 = new Mock<IStep>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create("Frank"))
                .Returns<string>(null);

            var requestStep = new Mock<IRequestStep>(MockBehavior.Strict);
            requestStep.Setup(rs => rs.ReuseHttpMessageHandler).Returns(false);
            requestStep.Setup(rs => rs.ClientName).Returns("Xi");
            requestStep.Setup(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), It.IsAny<AsyncPolicy<HttpResponseMessage>>()));

            var step2 = requestStep.As<IStep>();
            stepFactory.Setup(f => f.Create("Mary"))
                .Returns<string>(s => step2.Object);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);

            // Verify
            Assert.ThrowsException<ArgumentException>(
                () => registry.ConfigureHttpClients(httpClientFactory.Object));
            requestStep.Verify(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), It.IsAny<AsyncPolicy<HttpResponseMessage>>()), Times.Never);
        }


        [TestMethod]
        public void ConfigureHttpClients_PolicyMissing_Throws()
        {
            var expectedConfig = new ClientConfig
            {
                Policies = new List<string> { "MissingValue" }
            };

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);

            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => Policy.NoOpAsync<HttpResponseMessage>());
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => expectedConfig);

            // Create Moq steps
            var step1 = new Mock<IStep>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create("Frank"))
                .Returns<string>(null);

            var requestStep = new Mock<IRequestStep>(MockBehavior.Strict);
            requestStep.Setup(rs => rs.ReuseHttpMessageHandler).Returns(false);
            requestStep.Setup(rs => rs.ClientName).Returns("Xi");
            requestStep.Setup(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), It.IsAny<AsyncPolicy<HttpResponseMessage>>()));

            var step2 = requestStep.As<IStep>();
            stepFactory.Setup(f => f.Create("Mary"))
                .Returns<string>(s => step2.Object);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);

            // Verify
            Assert.ThrowsException<InvalidOperationException>(
                () => registry.ConfigureHttpClients(httpClientFactory.Object));
            requestStep.Verify(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), It.IsAny<AsyncPolicy<HttpResponseMessage>>()), Times.Never);
        }


        [TestMethod]
        public void ConfigureHttpClients_PolicyValueNull_Throws()
        {
            var expectedConfig = new ClientConfig
            {
                Policies = new List<string> { "Amanda" }
            };

            // Create SF.Mock settings
            var settings = CreateDefaultSettings();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IStepFactory>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<Processor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IPolicyFactory>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);

            processorFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            policyFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(null);
            clientFactory.Setup(f => f.Create(It.IsAny<string>()))
                .Returns<string>(s => expectedConfig);

            // Create Moq steps
            var step1 = new Mock<IStep>(MockBehavior.Strict);
            stepFactory.Setup(f => f.Create("Frank"))
                .Returns<string>(null);

            var requestStep = new Mock<IRequestStep>(MockBehavior.Strict);
            requestStep.Setup(rs => rs.ReuseHttpMessageHandler).Returns(false);
            requestStep.Setup(rs => rs.ClientName).Returns("Xi");
            requestStep.Setup(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), It.IsAny<AsyncPolicy<HttpResponseMessage>>()));

            var step2 = requestStep.As<IStep>();
            stepFactory.Setup(f => f.Create("Mary"))
                .Returns<string>(s => step2.Object);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(new Mock<ILogger>().Object);

            // Act
            var registry = new Registry(
                settings, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);

            // Verify
            Assert.ThrowsException<InvalidOperationException>(
                () => registry.ConfigureHttpClients(httpClientFactory.Object));
            requestStep.Verify(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), It.IsAny<AsyncPolicy<HttpResponseMessage>>()), Times.Never);
        }


        private ConfigurationSettings CreateDefaultSettings()
        {
            // Create configuration structures
            var configurations = new ConfigurationSectionCollection();
            var settings = MockConfigurationPackage.CreateConfigurationSettings(configurations);

            // Add sections
            var processorSection = MockConfigurationPackage.CreateConfigurationSection(Registry.ProcessorsSection);
            var stepSection = MockConfigurationPackage.CreateConfigurationSection(Registry.StepsSection);
            var policiesSection = MockConfigurationPackage.CreateConfigurationSection(Registry.PoliciesSection);
            var clientsSection = MockConfigurationPackage.CreateConfigurationSection(Registry.ClientsSection);
            configurations.Add(processorSection);
            configurations.Add(stepSection);
            configurations.Add(policiesSection);
            configurations.Add(clientsSection);

            // Add processors parameters
            ConfigurationProperty processor = MockConfigurationPackage
                .CreateConfigurationSectionParameters("Bob", "Bob");
            processorSection.Parameters.Add(processor);

            // Add step parameters
            ConfigurationProperty step1 = MockConfigurationPackage
                .CreateConfigurationSectionParameters("Frank", "Frank");
            ConfigurationProperty step2 = MockConfigurationPackage
                .CreateConfigurationSectionParameters("Mary", "Mary");
            stepSection.Parameters.Add(step1);
            stepSection.Parameters.Add(step2);

            // Add policy parameters
            ConfigurationProperty policy1 = MockConfigurationPackage
                .CreateConfigurationSectionParameters("Amanda", "Amanda");
            ConfigurationProperty policy2 = MockConfigurationPackage
                .CreateConfigurationSectionParameters("Max", "Max");
            ConfigurationProperty policy3 = MockConfigurationPackage
                .CreateConfigurationSectionParameters("Ted", "Ted");
            policiesSection.Parameters.Add(policy1);
            policiesSection.Parameters.Add(policy2);
            policiesSection.Parameters.Add(policy3);

            // Add clients parameters
            ConfigurationProperty client = MockConfigurationPackage
                .CreateConfigurationSectionParameters("Xi", "Xi");
            clientsSection.Parameters.Add(client);

            return settings;
        }
    }
}
