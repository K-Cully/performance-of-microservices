using CoreService.Controllers;
using CoreService.Model;
using CoreService.Simulation;
using CoreService.Simulation.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreService.Test.Controllers
{
    [TestClass]
    public class AdaptableControllerUnitTests
    {
        // TODO: write UTs
        private OkObjectResult okResult = new OkObjectResult(new SuccessResponse("success"));


        [TestMethod]
        [TestCategory("Input")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Throws_WhenPassedNull()
        {
            _ = new AdaptableController(null);
        }


        [TestMethod]
        [TestCategory("Input")]
        public async Task Get_WithNullName_ReturnsBadRequest()
        {
            // Setup
            Mock<IEngine> engine = new Mock<IEngine>(MockBehavior.Strict);
            AdaptableController controller = new AdaptableController(engine.Object);

            // Act
            IActionResult result = await controller.Get(null, "test").ConfigureAwait(false);

            // Verify
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult), "Result should be a bad request object");
            var resultObject = result as BadRequestObjectResult;
            Assert.IsInstanceOfType(resultObject.Value, typeof(ErrorResponse), "Value should be an error response");
            var response = resultObject.Value as ErrorResponse;
            Assert.AreEqual("name is required", response.Error, "Error should be set correctly");
        }


        [TestMethod]
        [TestCategory("Input")]
        public async Task Options_WithNullName_ReturnsBadRequest()
        {
            // Setup
            Mock<IEngine> engine = new Mock<IEngine>(MockBehavior.Strict);
            AdaptableController controller = new AdaptableController(engine.Object);

            // Act
            IActionResult result = await controller.Options(null, null).ConfigureAwait(false);

            // Verify
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult), "Result should be a bad request object");
            var resultObject = result as BadRequestObjectResult;
            Assert.IsInstanceOfType(resultObject.Value, typeof(ErrorResponse), "Value should be an error response");
            var response = resultObject.Value as ErrorResponse;
            Assert.AreEqual("name is required", response.Error, "Error should be set correctly");
        }


        [TestMethod]
        [TestCategory("Input")]
        public async Task Post_WithNullName_ReturnsBadRequest()
        {
            // Setup
            Mock<IEngine> engine = new Mock<IEngine>(MockBehavior.Strict);
            AdaptableController controller = new AdaptableController(engine.Object);
            var request = new AdaptableRequest();

            // Act
            IActionResult result = await controller.Post(null, request, "test").ConfigureAwait(false);

            // Verify
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult), "Result should be a bad request object");
            var resultObject = result as BadRequestObjectResult;
            Assert.IsInstanceOfType(resultObject.Value, typeof(ErrorResponse), "Value should be an error response");
            var response = resultObject.Value as ErrorResponse;
            Assert.AreEqual("name is required", response.Error, "Error should be set correctly");
        }


        [TestMethod]
        [TestCategory("Input")]
        public async Task Post_WithNullRequest_ReturnsBadRequest()
        {
            // Setup
            Mock<IEngine> engine = new Mock<IEngine>(MockBehavior.Strict);
            AdaptableController controller = new AdaptableController(engine.Object);

            // Act
            IActionResult result = await controller.Post("test", null, "test").ConfigureAwait(false);

            // Verify
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult), "Result should be a bad request object");
            var resultObject = result as BadRequestObjectResult;
            Assert.IsInstanceOfType(resultObject.Value, typeof(ErrorResponse), "Value should be an error response");
            var response = resultObject.Value as ErrorResponse;
            Assert.AreEqual("request is required", response.Error, "Error should be set correctly");
        }


        [TestMethod]
        [TestCategory("Input")]
        public async Task Put_WithNullName_ReturnsBadRequest()
        {
            // Setup
            Mock<IEngine> engine = new Mock<IEngine>(MockBehavior.Strict);
            AdaptableController controller = new AdaptableController(engine.Object);
            var request = new AdaptableRequest();

            // Act
            IActionResult result = await controller.Put(null, request, "test").ConfigureAwait(false);

            // Verify
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult), "Result should be a bad request object");
            var resultObject = result as BadRequestObjectResult;
            Assert.IsInstanceOfType(resultObject.Value, typeof(ErrorResponse), "Value should be an error response");
            var response = resultObject.Value as ErrorResponse;
            Assert.AreEqual("name is required", response.Error, "Error should be set correctly");
        }


        [TestMethod]
        [TestCategory("Input")]
        public async Task Put_WithNullRequest_ReturnsBadRequest()
        {
            // Setup
            Mock<IEngine> engine = new Mock<IEngine>(MockBehavior.Strict);
            AdaptableController controller = new AdaptableController(engine.Object);

            // Act
            IActionResult result = await controller.Put("test", null, "test").ConfigureAwait(false);

            // Verify
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult), "Result should be a bad request object");
            var resultObject = result as BadRequestObjectResult;
            Assert.IsInstanceOfType(resultObject.Value, typeof(ErrorResponse), "Value should be an error response");
            var response = resultObject.Value as ErrorResponse;
            Assert.AreEqual("request is required", response.Error, "Error should be set correctly");
        }




    }
}
