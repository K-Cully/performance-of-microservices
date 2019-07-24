using CoreService.Controllers;
using CoreService.Simulation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace CoreService.Test.Controllers
{
    [TestClass]
    public class AdaptableControllerUnitTests
    {
        // TODO: write UTs

        [TestMethod]
        public void Dummy()
        {
            AdaptableController controller = new AdaptableController(new Engine(new Registry()));

            Assert.IsTrue(false);
        }
    }
}
