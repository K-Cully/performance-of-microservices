using CoreService.Simulation.Core;
using CoreService.Simulation.Processors;
using CoreService.Simulation.Steps;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceFabric.Mocks;
using System;
using System.Collections.Generic;
using System.Fabric;
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
                () => _ = new Registry(null, new StepFactory(), new ProcessorFactory()));
        }


        [TestMethod]
        public void Constructor_Throws_WhenStepFactoryIsNull()
        {
            var configurations = new ConfigurationSectionCollection();
            var settings = MockConfigurationPackage.CreateConfigurationSettings(configurations);

            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(settings, null, new ProcessorFactory()));
        }


        [TestMethod]
        public void Constructor_Throws_WhenProcessorFactoryIsNull()
        {
            var configurations = new ConfigurationSectionCollection();
            var settings = MockConfigurationPackage.CreateConfigurationSettings(configurations);

            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new Registry(settings, new StepFactory(), null));
        }


        [TestMethod]
        public void Constructor_DoesNotCallFactories_WhenSettingsAreMissing()
        {
            //Registry registry = new Registry();

            Assert.IsTrue(false);
        }


        [TestMethod]
        public void Constructor_CallsFactoriesCorrectly_WhenSettingsArePresent()
        {
            //Registry registry = new Registry();

            Assert.IsTrue(false);
        }



        private void Create()
        {
            // TODO: remvove/ update

            // Create configuration structures
            var configurations = new ConfigurationSectionCollection();
            var settings = MockConfigurationPackage.CreateConfigurationSettings(configurations);
            var section = MockConfigurationPackage.CreateConfigurationSection("Processors");
            configurations.Add(section);

            // Add Parameter entries
            ConfigurationProperty processor1 = MockConfigurationPackage
                .CreateConfigurationSectionParameters("1", "{ errorSize: 100, latency: 0, steps: ['A'], successSize: 20 }");
            section.Parameters.Add(processor1);
            ConfigurationProperty processor2 = MockConfigurationPackage
                .CreateConfigurationSectionParameters("2", "{ errorSize : 15, latency : 0, steps : [ 'B', 'A' ], successSize : 0 }");
            section.Parameters.Add(processor2);

            // TODO: remove?
            // Register settings
            ConfigurationPackage configPackage = MockConfigurationPackage.
                CreateConfigurationPackage(settings, "");

        }
    }
}
