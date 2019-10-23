using ClusterEmulator.Service.Simulation.Core;
using ClusterEmulator.Service.Simulation.HttpClientConfiguration;
using ClusterEmulator.Service.Simulation.Processors;
using ClusterEmulator.Service.Simulation.Steps;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Polly;
using Polly.Wrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace ClusterEmulator.Service.Simulation.Test.Core
{
    [TestClass]
    public class RegistryUnitTests
    {
        [TestMethod]
        public void Constructor_Throws_WhenConfigurationSettingsIsNull()
        {
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
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
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);

            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(settings.Object, null, processorFactory.Object, policyFactory.Object,
                                clientFactory.Object, logger.Object, loggerFactory.Object));
        }


        [TestMethod]
        public void Constructor_Throws_WhenProcessorFactoryIsNull()
        {
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(settings.Object, stepFactory.Object, null, policyFactory.Object,
                                clientFactory.Object, logger.Object, loggerFactory.Object));
        }


        [TestMethod]
        public void Constructor_Throws_WhenPolicyFactoryIsNull()
        {
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(settings.Object, stepFactory.Object, processorFactory.Object,
                                null, clientFactory.Object, logger.Object, loggerFactory.Object));
        }


        [TestMethod]
        public void Constructor_Throws_WhenClientFactoryIsNull()
        {
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(settings.Object, stepFactory.Object, processorFactory.Object,
                                policyFactory.Object, null, logger.Object, loggerFactory.Object));
        }


        [TestMethod]
        public void Constructor_Throws_WhenLoggerIsNull()
        {
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(settings.Object, stepFactory.Object, processorFactory.Object,
                                policyFactory.Object, clientFactory.Object, null, loggerFactory.Object));
        }


        [TestMethod]
        public void Constructor_Throws_WhenLoggerFActoryIsNull()
        {
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);

            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(settings.Object, stepFactory.Object, processorFactory.Object,
                                policyFactory.Object, clientFactory.Object, logger.Object, null));
        }


        [TestMethod]
        public void Constructor_Throws_WhenSettingsSectionsAreMissing()
        {
            // Create Moq proxy instances
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            // Add processors
            IEnumerable<KeyValuePair<string, string>> outValues = null;
            settings.Setup(s => s.TryGetSection(It.IsAny<string>(), out outValues))
                .Returns(false);

            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
            var clientFactory = new Mock<IConfigFactory<ClientConfig>>(MockBehavior.Strict);
            var logger = new Mock<ILogger<Registry>>(MockBehavior.Loose);
            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);

            // Act
            Assert.ThrowsException<InvalidOperationException>(()
                => new Registry(settings.Object, stepFactory.Object, processorFactory.Object,
                    policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object));

            // Verify
            stepFactory.Verify(f => f.Create(It.IsAny<string>()), Times.Never);
            processorFactory.Verify(f => f.Create(It.IsAny<string>()), Times.Never);
            policyFactory.Verify(f => f.Create(It.IsAny<string>()), Times.Never);
        }


        [TestMethod]
        public void Constructor_CallsFactoriesCorrectly_WhenSettingsArePresent()
        {
            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            IEnumerable<KeyValuePair<string, string>> outList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("name", null)
            };
            settings.Setup(s => s.TryGetSection(It.IsAny<string>(), out outList))
                .Returns(true);

            // Create Moq proxy instances
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);
            var policyRegistry = registry.PolicyRegistry;
            var clients = new List<KeyValuePair<string, ClientConfig>>(registry.Clients);

            // Verify
            stepFactory.Verify(f => f.Create(It.IsAny<string>()), Times.Exactly(1));
            processorFactory.Verify(f => f.Create(It.IsAny<string>()), Times.Exactly(1));
            policyFactory.Verify(f => f.Create(It.IsAny<string>()), Times.Exactly(1));
            clientFactory.Verify(f => f.Create(It.IsAny<string>()), Times.Exactly(1));
            Assert.IsNotNull(policyRegistry);
            Assert.AreEqual(1, policyRegistry.Count);
            Assert.IsNotNull(clients);
            Assert.AreEqual(1, clients.Count);
            Assert.AreEqual("name", clients[0].Key);
            Assert.AreEqual(null, clients[0].Value);
        }


        [TestMethod]
        public void GetRequestProcessor_ReturnsCorrectValue_WhenRegistered()
        {
            string processorName = "Bob";
            RequestProcessor expectedProcessor = new RequestProcessor();

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            IEnumerable<KeyValuePair<string, string>> outList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(processorName, "setting")
            };
            settings.Setup(s => s.TryGetSection(It.IsAny<string>(), out outList))
                .Returns(true);

            // Create Moq proxy instances
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);
            IRequestProcessor processor = registry.GetRequestProcessor(processorName);

            // Verify
            Assert.IsNotNull(processor, "GetRequestProcessor should return the registered value");
        }


        [TestMethod]
        public void GetRequestProcessor_Throws_WhenRegisteredValueIsNull()
        {
            string processorName = "Bob";

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            IEnumerable<KeyValuePair<string, string>> outList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(processorName, "setting")
            };
            settings.Setup(s => s.TryGetSection(It.IsAny<string>(), out outList))
                .Returns(true);

            // Create Moq proxy instances
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);

            // Verify
            Assert.ThrowsException<InvalidOperationException>(() => registry.GetRequestProcessor(processorName));
        }


        [TestMethod]
        public void GetRequestProcessor_Throws_WhenNameIsNotRegistered()
        {
            string processorName = "Xi";
            RequestProcessor expectedProcessor = new RequestProcessor();

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            settings.SetupSettingDefaults();

            // Create Moq proxy instances
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);

            // Verify
            Assert.ThrowsException<InvalidOperationException>(() => registry.GetRequestProcessor(processorName));
        }


        [TestMethod]
        public void GetRequestProcessor_Throws_WhenNameIsNull()
        {
            string processorName = null;
            RequestProcessor expectedProcessor = new RequestProcessor();

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            settings.SetupSettingDefaults();

            // Create Moq proxy instances
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);

            // Verify
            Assert.ThrowsException<ArgumentException>(() => registry.GetRequestProcessor(processorName));
        }


        [TestMethod]
        public void GetRequestProcessor_Throws_WhenNameIsOfWrongType()
        {
            string processorName = "Bob";
            IProcessor expectedProcessor = new StartupProcessor();

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            IEnumerable<KeyValuePair<string, string>> outList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(processorName, "setting")
            };
            settings.Setup(s => s.TryGetSection(It.IsAny<string>(), out outList))
                .Returns(true);

            // Create Moq proxy instances
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);

            // Verify
            Assert.ThrowsException<InvalidOperationException>(() => registry.GetRequestProcessor(processorName));
        }


        [TestMethod]
        public void GetStartupProcessors_ReturnsInitializedList_WhenStartUpProcessorExists()
        {
            string processorName = "Bob";
            IProcessor expectedProcessor = new StartupProcessor();

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            IEnumerable<KeyValuePair<string, string>> outList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(processorName, "setting")
            };
            settings.Setup(s => s.TryGetSection(It.IsAny<string>(), out outList))
                .Returns(true);

            // Create Moq proxy instances
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);
            var query = registry.GetStartupProcessors();

            // Verify
            Assert.IsNotNull(query);
            var processors = query.ToList();
            Assert.AreEqual(1, processors.Count);
            Assert.IsInstanceOfType(processors.First(), typeof(IStartupProcessor));
        }


        [TestMethod]
        public void GetStartupProcessors_ReturnsEmptyList_WhenStartUpProcessorDoNotExists()
        {
            string processorName = "Bob";
            IProcessor expectedProcessor = new RequestProcessor();

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            IEnumerable<KeyValuePair<string, string>> outList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(processorName, "setting")
            };
            settings.Setup(s => s.TryGetSection(It.IsAny<string>(), out outList))
                .Returns(true);

            // Create Moq proxy instances
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);
            var query = registry.GetStartupProcessors();

            // Verify
            Assert.IsNotNull(query);
            var processors = query.ToList();
            Assert.AreEqual(0, processors.Count);
        }


        [TestMethod]
        public void GetStep_ReturnsCorrectValue_WhenRegistered()
        {
            string stepName = "Mary";

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            IEnumerable<KeyValuePair<string, string>> outList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(stepName, "setting")
            };
            settings.Setup(s => s.TryGetSection(It.IsAny<string>(), out outList))
                .Returns(true);

            // Create Moq proxy instances
            Mock<IStep> stepMock = new Mock<IStep>(MockBehavior.Strict);
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);
            IStep step = registry.GetStep(stepName);

            // Verify
            Assert.IsNotNull(step, "GetStep should return the registered value");
        }


        [TestMethod]
        public void GetPolicy_ReturnsCorrectValue_WhenRegistered()
        {
            string policyName = "Max";
            var expectedPolicy = Policy.NoOpAsync<HttpResponseMessage>();

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            IEnumerable<KeyValuePair<string, string>> outList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(policyName, "setting")
            };
            settings.Setup(s => s.TryGetSection(It.IsAny<string>(), out outList))
                .Returns(true);

            // Create Moq proxy instances
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);
            IAsyncPolicy<HttpResponseMessage> policy = registry.GetPolicy(policyName);

            // Verify
            Assert.IsNotNull(policy, "GetPolicy should return the registered value");
        }


        [TestMethod]
        public void GetClient_ReturnsCorrectValue_WhenRegistered()
        {
            string clientName = "Xi";
            var expectedConfig = new ClientConfig();

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            IEnumerable<KeyValuePair<string, string>> outList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(clientName, "setting")
            };
            settings.Setup(s => s.TryGetSection(It.IsAny<string>(), out outList))
                .Returns(true);

            // Create Moq proxy instances
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);
            ClientConfig config = registry.GetClient(clientName);

            // Verify
            Assert.IsNotNull(config, "GetClient should return the registered value");
        }


        [TestMethod]
        public void ConfigureHttpClients_NullFactory_Throws()
        {
            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            IEnumerable<KeyValuePair<string, string>> outList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("name", "setting")
            };
            settings.Setup(s => s.TryGetSection(It.IsAny<string>(), out outList))
                .Returns(true);

            // Create Moq proxy instances
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);

            // Verify
            Assert.ThrowsException<ArgumentNullException>(
                () => registry.ConfigureHttpClients(null));
        }


        [TestMethod]
        public void ConfigureHttpClients_NoRequestSteps_DoesNothing()
        {
            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            IEnumerable<KeyValuePair<string, string>> outList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("name", "setting")
            };
            settings.Setup(s => s.TryGetSection(It.IsAny<string>(), out outList))
                .Returns(true);

            // Create Moq proxy instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
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

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            settings.SetupSettingDefaults();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
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

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            settings.SetupSettingDefaults();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
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

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            settings.SetupSettingDefaults();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);

            // Verify
            Assert.ThrowsException<InvalidOperationException>(
                () => registry.ConfigureHttpClients(httpClientFactory.Object));
            requestStep.Verify(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), null), Times.Never);
        }


        [TestMethod]
        public void ConfigureHttpClients_NullClient_Throws()
        {
            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            settings.SetupSettingDefaults();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
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

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            settings.SetupSettingDefaults();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
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

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            settings.SetupSettingDefaults();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
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

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            settings.SetupSettingDefaults();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
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

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            settings.SetupSettingDefaults();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
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

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            settings.SetupSettingDefaults();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
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

            // Create settings
            var settings = new Mock<IRegistrySettings>(MockBehavior.Strict);
            settings.SetupSettingDefaults();

            // Create Moq factory instances
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var stepFactory = new Mock<IConfigFactory<IStep>>(MockBehavior.Strict);
            var processorFactory = new Mock<IConfigFactory<IProcessor>>(MockBehavior.Strict);
            var policyFactory = new Mock<IConfigFactory<IAsyncPolicy<HttpResponseMessage>>>(MockBehavior.Strict);
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
                settings.Object, stepFactory.Object, processorFactory.Object,
                policyFactory.Object, clientFactory.Object, logger.Object, loggerFactory.Object);

            // Verify
            Assert.ThrowsException<InvalidOperationException>(
                () => registry.ConfigureHttpClients(httpClientFactory.Object));
            requestStep.Verify(rs => rs.Configure(It.IsAny<IHttpClientFactory>(), It.IsAny<AsyncPolicy<HttpResponseMessage>>()), Times.Never);
        }
    }

    internal static class MyMock
    {
        internal static void SetupSettingDefaults(this Mock<IRegistrySettings> mock)
        {
            // Add processors
            IEnumerable<KeyValuePair<string, string>> processors = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Bob", "Bob")
            };
            mock.Setup(s => s.TryGetSection(Registry.ProcessorsSection, out processors))
                .Returns(true);

            // Add steps
            IEnumerable<KeyValuePair<string, string>> steps = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Frank", "Frank"),
                new KeyValuePair<string, string>("Mary", "Mary")
            };
            mock.Setup(s => s.TryGetSection(Registry.StepsSection, out steps))
                .Returns(true);

            // Add policies
            IEnumerable<KeyValuePair<string, string>> policies = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Amanda", "Amanda"),
                new KeyValuePair<string, string>("Max", "Max"),
                new KeyValuePair<string, string>("Ted", "Ted")
            };
            mock.Setup(s => s.TryGetSection(Registry.PoliciesSection, out policies))
                .Returns(true);

            // Add clients
            IEnumerable<KeyValuePair<string, string>> clients = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Xi", "Xi")
            };
            mock.Setup(s => s.TryGetSection(Registry.ClientsSection, out clients))
                .Returns(true);
        }
    }
}
