﻿using ClusterEmulator.ServiceFabric.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceFabric.Mocks;
using System;
using System.Fabric;
using System.Fabric.Description;
using static ServiceFabric.Mocks.MockConfigurationPackage;

namespace ClusterEmulator.ServiceFabric.Test.Extensions
{
    [TestClass]
    public class ServiceCollectionExtensionsUnitTests
    {
        [TestMethod]
        public void AddSimulationSettings_NullCollection_Throws()
        {
            // Arrange
            var context = MockStatelessServiceContextFactory.Default;

            // Act & Verify
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = ServiceCollectionExtensions.AddSimulationSettings(null, context));
        }


        [TestMethod]
        public void AddSimulationSettings_NullContext_Throws()
        {
            // Arrange
            var serviceCollection = new Mock<IServiceCollection>(MockBehavior.Loose);

            // Act & Verify
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = ServiceCollectionExtensions.AddSimulationSettings(serviceCollection.Object, null));
        }


        [TestMethod]
        public void AddSimulationSettings_ValidContent_ReturnsCorrectly()
        {
            // Initialize config
            var configSections = new ConfigurationSectionCollection();
            var configSettings = CreateConfigurationSettings(configSections);
            ConfigurationSection configSection = CreateConfigurationSection(nameof(configSection.Name));
            configSections.Add(configSection);
            ConfigurationProperty parameter = CreateConfigurationSectionParameters(nameof(parameter.Name), nameof(parameter.Value));
            configSection.Parameters.Add(parameter);
            ConfigurationPackage configPackage = CreateConfigurationPackage(configSettings, nameof(configPackage.Path));
            var codePackageContext = new Mock<ICodePackageActivationContext>(MockBehavior.Loose);
            codePackageContext.Setup(cp => cp.GetConfigurationPackageObject("Config"))
                .Returns(configPackage);

            // Initialize service context
            var newUri = new Uri("fabric:/MockApp/OtherMockStatelessService");
            var serviceTypeName = "OtherMockServiceType";
            var partitionId = Guid.NewGuid();
            var replicaId = long.MaxValue;
            var serviceContext = MockStatelessServiceContextFactory.Create(
                codePackageContext.Object, serviceTypeName, newUri, partitionId, replicaId);

            var services = new ServiceCollection();

            // Act
            IServiceCollection collection = services.AddSimulationSettings(serviceContext);

            // Verify
            Assert.IsNotNull(collection);
            Assert.AreEqual(2, collection.Count);
        }
    }
}
