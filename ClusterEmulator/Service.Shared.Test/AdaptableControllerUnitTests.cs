using ClusterEmulator.Service.Models;
using ClusterEmulator.Service.Simulation.Core;
using ClusterEmulator.Service.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace CoreService.Test.Controllers
{
    [TestClass]
    public class AdaptableControllerUnitTests
    {
        private readonly OkObjectResult okResult = new OkObjectResult(new SuccessResponse(100));


        [TestMethod]
        [TestCategory("Input")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Throws_WhenPassedNullLogger()
        {
            // Setup
            var engine = new Mock<IEngine>(MockBehavior.Strict);

            // Act
            _ = new AdaptableController(null, engine.Object);
        }


        [TestMethod]
        [TestCategory("Input")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Throws_WhenPassedNullEngine()
        {
            // Setup
            var logger = new Mock<ILogger<AdaptableController>>(MockBehavior.Strict);

            // Act
            _ = new AdaptableController(logger.Object, null);
        }


        [TestMethod]
        [TestCategory("Input")]
        public async Task Get_WithNullName_ReturnsBadRequest()
        {
            // Setup
            var engine = new Mock<IEngine>(MockBehavior.Strict);
            var logger = new Mock<ILogger<AdaptableController>>(MockBehavior.Loose);
            var controller = new AdaptableController(logger.Object, engine.Object);

            // Act
            IActionResult result = await controller.Get(null, "test").ConfigureAwait(false);

            // Verify
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult), "Result should be a bad result object");
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
            var engine = new Mock<IEngine>(MockBehavior.Strict);
            var logger = new Mock<ILogger<AdaptableController>>(MockBehavior.Loose);
            var controller = new AdaptableController(logger.Object, engine.Object);

            // Act
            IActionResult result = await controller.Options(null, null).ConfigureAwait(false);

            // Verify
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult), "Result should be a bad result object");
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
            var engine = new Mock<IEngine>(MockBehavior.Strict);
            var logger = new Mock<ILogger<AdaptableController>>(MockBehavior.Loose);
            var controller = new AdaptableController(logger.Object, engine.Object);
            var request = new AdaptableRequest();

            // Act
            IActionResult result = await controller.Post(null, request, "test").ConfigureAwait(false);

            // Verify
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult), "Result should be a bad result object");
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
            var engine = new Mock<IEngine>(MockBehavior.Strict);
            var logger = new Mock<ILogger<AdaptableController>>(MockBehavior.Loose);
            var controller = new AdaptableController(logger.Object, engine.Object);

            // Act
            IActionResult result = await controller.Post("test", null, "test").ConfigureAwait(false);

            // Verify
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult), "Result should be a bad result object");
            var resultObject = result as BadRequestObjectResult;
            Assert.IsInstanceOfType(resultObject.Value, typeof(ErrorResponse), "Value should be an error response");
            var response = resultObject.Value as ErrorResponse;
            Assert.AreEqual("request is required", response.Error, "Error should be set correctly");
        }


        [TestMethod]
        [TestCategory("Input")]
        public async Task Post_WithInvalidModel_ReturnsBadRequest()
        {
            // Setup
            var engine = new Mock<IEngine>(MockBehavior.Strict);
            var logger = new Mock<ILogger<AdaptableController>>(MockBehavior.Loose);
            var controller = new AdaptableController(logger.Object, engine.Object);
            controller.ModelState.AddModelError("value_1", "value_1 is not valid");
            controller.ModelState.AddModelError("value_2", "value_2 is not valid");

            // Act
            IActionResult result = await controller.Post("test", null, "test").ConfigureAwait(false);

            // Verify
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult), "Result should be a bad result object");
            var resultObject = result as BadRequestObjectResult;
            Assert.IsInstanceOfType(resultObject.Value, typeof(ErrorResponse), "Value should be an error response");
            var response = resultObject.Value as ErrorResponse;
            Assert.AreEqual("['value_1 is not valid', 'value_2 is not valid']", response.Error, "Error should be set correctly");
        }



        [TestMethod]
        [TestCategory("Input")]
        public async Task Put_WithNullName_ReturnsBadRequest()
        {
            // Setup
            var engine = new Mock<IEngine>(MockBehavior.Strict);
            var logger = new Mock<ILogger<AdaptableController>>(MockBehavior.Loose);
            var controller = new AdaptableController(logger.Object, engine.Object);
            var request = new AdaptableRequest();

            // Act
            IActionResult result = await controller.Put(null, request, "test").ConfigureAwait(false);

            // Verify
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult), "Result should be a bad result object");
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
            var engine = new Mock<IEngine>(MockBehavior.Strict);
            var logger = new Mock<ILogger<AdaptableController>>(MockBehavior.Loose);
            var controller = new AdaptableController(logger.Object, engine.Object);

            // Act
            IActionResult result = await controller.Put("test", null, "test").ConfigureAwait(false);

            // Verify
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult), "Result should be a bad result object");
            var resultObject = result as BadRequestObjectResult;
            Assert.IsInstanceOfType(resultObject.Value, typeof(ErrorResponse), "Value should be an error response");
            var response = resultObject.Value as ErrorResponse;
            Assert.AreEqual("request is required", response.Error, "Error should be set correctly");
        }


        [TestMethod]
        [TestCategory("Input")]
        public async Task Put_WithInvalidModel_ReturnsBadRequest()
        {
            // Setup
            var engine = new Mock<IEngine>(MockBehavior.Strict);
            var logger = new Mock<ILogger<AdaptableController>>(MockBehavior.Loose);
            var controller = new AdaptableController(logger.Object, engine.Object);
            controller.ModelState.AddModelError("value_1", "value_1 is not valid");

            // Act
            IActionResult result = await controller.Post("test", null, "test").ConfigureAwait(false);

            // Verify
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult), "Result should be a bad result object");
            var resultObject = result as BadRequestObjectResult;
            Assert.IsInstanceOfType(resultObject.Value, typeof(ErrorResponse), "Value should be an error response");
            var response = resultObject.Value as ErrorResponse;
            Assert.AreEqual("['value_1 is not valid']", response.Error, "Error should be set correctly");
        }


        [TestMethod]
        [TestCategory("Functional")]
        public async Task Get_WithValidData_ReturnsCorrectResponse()
        {
            string name = "Bob";

            // Setup
            var engine = new Mock<IEngine>(MockBehavior.Strict);
            engine.Setup(e => e.ProcessRequestAsync(name))
                .ReturnsAsync(okResult);
            var logger = new Mock<ILogger<AdaptableController>>(MockBehavior.Loose);
            var controller = new AdaptableController(logger.Object, engine.Object);

            // Act
            IActionResult result = await controller.Get(name, "test").ConfigureAwait(false);

            // Verify
            Assert.IsInstanceOfType(result, typeof(OkObjectResult), "Result should be an Ok result object");
            var resultObject = result as OkObjectResult;
            Assert.AreEqual(okResult, resultObject, "Result should match expected");
        }


        [TestMethod]
        [TestCategory("Functional")]
        public async Task Get_WhenEngineThrowsArgumentException_ReturnsBadRequest()
        {
            string message = "boom";

            // Setup
            var engine = new Mock<IEngine>(MockBehavior.Strict);
            engine.Setup(e => e.ProcessRequestAsync(It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException(message));
            var logger = new Mock<ILogger<AdaptableController>>(MockBehavior.Loose);

            var controller = new AdaptableController(logger.Object, engine.Object);

            // Act
            IActionResult result = await controller.Get("test", "test").ConfigureAwait(false);

            // Verify
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult), "Result should be a bad request result object");
            var resultObject = result as BadRequestObjectResult;
            Assert.IsInstanceOfType(resultObject.Value, typeof(ErrorResponse), "Value should be an error response");
            var response = resultObject.Value as ErrorResponse;
            Assert.AreEqual(message, response.Error, "Error should be set correctly");
        }


        [TestMethod]
        [TestCategory("Functional")]
        public async Task Get_WhenEngineThrowsException_ReturnsServerError()
        {
            string message = "boom";

            // Setup
            var engine = new Mock<IEngine>(MockBehavior.Strict);
            engine.Setup(e => e.ProcessRequestAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception(message));
            var logger = new Mock<ILogger<AdaptableController>>(MockBehavior.Loose);
            var controller = new AdaptableController(logger.Object, engine.Object);

            // Act
            IActionResult result = await controller.Get("test", "test").ConfigureAwait(false);

            // Verify
            Assert.IsInstanceOfType(result, typeof(StatusCodeResult), "Result should be a status code result object");
            var statusCode = result as StatusCodeResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, statusCode.StatusCode,
                "Status code should be 500");
        }
    }
}
