using CoreService.Controllers;
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
        [TestMethod]
        public void Dummy()
        {
            AdaptableController controller = new AdaptableController();
        }
    }
}
