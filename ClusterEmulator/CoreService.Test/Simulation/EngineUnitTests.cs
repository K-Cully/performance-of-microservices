using CoreService.Simulation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CoreService.Test
{
    [TestClass]
    public class EngineUnitTests
    {
        [TestMethod]
        [TestCategory("InputTest")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Throws_WhenPassedNull()
        {
            _ = new Engine(null);
        }
    }
}
