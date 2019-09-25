using ClusterEmulator.Service.Simulation.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace CoreService.Test.Controllers
{
    [TestClass]
    public class FabricConfigurationSettingsUnitTests
    {
        [TestMethod]
        [TestCategory("Input")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Throws_WhenPassedNullLogger()
        {
            Assert.IsTrue(false);
        }
    }
}
