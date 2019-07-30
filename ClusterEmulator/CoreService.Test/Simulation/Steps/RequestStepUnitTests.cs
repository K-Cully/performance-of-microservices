using CoreService.Simulation.Steps;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreService.Test.Simulation.Steps
{
    [TestClass]
    public class RequestStepUnitTests
    {
        [TestMethod]
        public void Deserialization_ValidData_CreatesValidInstance()
        {
            ulong expectedBytes = 0;
            RequestStep step = JsonConvert.DeserializeObject<RequestStep>("{ url: http://localhost/, size : 10 }");

            Assert.AreEqual("http://localhost/", step.Url);
            Assert.AreEqual(10, step.PayloadSize);
        }


        [TestMethod]
        public void Deserialization_InvalidData_Throws()
        {
            Assert.ThrowsException<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<RequestStep>("{ }"));
        }


        [TestMethod]
        public async Task ExecuteAsync_MissingUrl_Throws()
        {
            var step = new RequestStep()
            { Url = string.Empty, PayloadSize = 15 };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_FileUrl_Throws()
        {
            var step = new RequestStep()
            { Url = "file://test.txt", PayloadSize = 15 };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_RelativeUrl_Throws()
        {
            var step = new RequestStep()
            { Url = "/test.html", PayloadSize = 15 };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_AbsoluteUrlSuccess_ReturnsSuccess()
        {
            // TODO: mock endpoint
            var step = new RequestStep()
            { Url = "http://test.html", PayloadSize = 15 };

            ExecutionStatus status = await step.ExecuteAsync();
            Assert.AreEqual(ExecutionStatus.Success, status);
        }


        [TestMethod]
        public async Task ExecuteAsync_AbsoluteUrlError_ReturnsFail()
        {
            // TODO: mock endpoint
            var step = new RequestStep()
            { Url = "http://test.html", PayloadSize = 15 };

            ExecutionStatus status = await step.ExecuteAsync();
            Assert.AreEqual(ExecutionStatus.Fail, status);
        }
    }
 }
