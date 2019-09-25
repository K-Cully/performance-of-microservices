using ClusterEmulator.Service.Simulation.Core;
using CoreService.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks;
using System;
using System.Collections.Generic;
using System.Fabric.Description;
using System.Linq;
using ConfigurationSectionCollection = ServiceFabric.Mocks.MockConfigurationPackage.ConfigurationSectionCollection;

namespace CoreService.Test.Controllers
{
    [TestClass]
    public class FabricConfigurationSettingsUnitTests
    {
        [TestMethod]
        public void Constructor_NullConfiguration_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new FabricConfigurationSettings(null));
        }


        [TestMethod]
        public void TryGetSection_NullName_ReturnsFalse()
        {
            // Arrange
            var configurations = new ConfigurationSectionCollection();
            var configuration = MockConfigurationPackage.CreateConfigurationSettings(configurations);
            var registrySettings = new FabricConfigurationSettings(configuration);

            // Act
            bool retrieved = registrySettings.TryGetSection(null, out IEnumerable<KeyValuePair<string, string>> settings);

            // Verify
            Assert.IsFalse(retrieved);
            Assert.IsNull(settings);
        }


        [TestMethod]
        public void TryGetSection_NullSection_ReturnsFalse()
        {
            // Arrange
            var configuration = MockConfigurationPackage.CreateConfigurationSettings(null);
            var registrySettings = new FabricConfigurationSettings(configuration);

            // Act
            bool retrieved = registrySettings.TryGetSection("test", out IEnumerable<KeyValuePair<string, string>> settings);

            // Verify
            Assert.IsFalse(retrieved);
            Assert.IsNull(settings);
        }


        [TestMethod]
        public void TryGetSection_SectionNotPresent_ReturnsFalse()
        {
            // Arrange
            var configuration = CreateDefaultSettings();
            var registrySettings = new FabricConfigurationSettings(configuration);

            // Act
            bool retrieved = registrySettings.TryGetSection("test", out IEnumerable<KeyValuePair<string, string>> settings);

            // Verify
            Assert.IsFalse(retrieved);
            Assert.IsNull(settings);
        }


        [TestMethod]
        public void TryGetSection_SectionPresent_ReturnsTrue_SetsListValues()
        {
            // Arrange
            var configuration = CreateDefaultSettings();
            var registrySettings = new FabricConfigurationSettings(configuration);

            // Act
            bool retrieved = registrySettings.TryGetSection(Registry.PoliciesSection,
                out IEnumerable<KeyValuePair<string, string>> settings);

            // Verify
            Assert.IsTrue(retrieved);
            Assert.IsNotNull(settings);
            Assert.AreEqual(3, settings.Count());
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
