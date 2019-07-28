using CoreService.Simulation.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace CoreService.Test.Simulation.Core
{
    [TestClass]
    public class RegistryUnitTests
    {
        // TODO: write UTs

        [TestMethod]
        public void Constructor_CallsFactoriesCorrectly_WhenSettingsArePresent()
        {
            //Registry registry = new Registry();

            Assert.IsTrue(false);
        }


        [TestMethod]
        public void Constructor_Throws_WhenConfigurationSettinsIsNull()
        {
            //Registry registry = new Registry();

            Assert.IsTrue(false);
        }
    }
}
