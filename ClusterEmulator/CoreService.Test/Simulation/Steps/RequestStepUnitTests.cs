using CoreService.Simulation.Steps;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace CoreService.Test.Simulation.Steps
{
    [TestClass]
    public class RequestStepUnitTests
    {
        //[TestMethod]
        //public void Deserialization_ValidData_CreatesValidInstance()
        //{
        //    RequestStep step = JsonConvert.DeserializeObject<RequestStep>(
        //        "{ method : GET, url : http://localhost/, size : 10, timeout: 3, retries : 0, retryPloicy : None }");

        //    Assert.AreEqual("http://localhost/", step.Url);
        //    Assert.AreEqual(10, step.PayloadSize);
        //}


        //[TestMethod]
        //public void Deserialization_InvalidData_Throws()
        //{
        //    Assert.ThrowsException<JsonSerializationException>(
        //        () => JsonConvert.DeserializeObject<RequestStep>("{ }"));
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_MissingUrl_Throws()
        //{
        //    var step = new RequestStep()
        //    { Method = "GET", Url = string.Empty, PayloadSize = 15, Timeout = 2, Retries = 1, RetryPolicy = None };

        //    await Assert.ThrowsExceptionAsync<InvalidOperationException>(
        //        () => step.ExecuteAsync());
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_FileUrl_Throws()
        //{
        //    var step = new RequestStep()
        //    { Method = "GET", Url = "file://test.txt", PayloadSize = 15, Timeout = 2, Retries = 1, RetryPolicy = None };

        //    await Assert.ThrowsExceptionAsync<InvalidOperationException>(
        //        () => step.ExecuteAsync());
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_RelativeUrl_Throws()
        //{
        //    var step = new RequestStep()
        //    { Method = "GET", Url = "/test.html", PayloadSize = 15, Timeout = 2, Retries = 1, RetryPolicy = None };

        //    await Assert.ThrowsExceptionAsync<InvalidOperationException>(
        //        () => step.ExecuteAsync());
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_InvalidPayloadSize_Throws()
        //{
        //    var step = new RequestStep()
        //    { Method = "GET", Url = "http://test.html", PayloadSize = -1, Timeout = 1, Retries = 1, RetryPolicy = None };

        //    await Assert.ThrowsExceptionAsync<InvalidOperationException>(
        //        () => step.ExecuteAsync());
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_MissingPayloadSize_Throws()
        //{
        //    var step = new RequestStep()
        //    { Method = "GET", Url = "http://test.html", Timeout = 15, Retries = 1, RetryPolicy = None };

        //    await Assert.ThrowsExceptionAsync<InvalidOperationException>(
        //        () => step.ExecuteAsync());
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_InvalidTimeout_Throws()
        //{
        //    var step = new RequestStep()
        //    { Method = "GET", Url = "http://test.html", PayloadSize = 15, Timeout = -1, Retries = 1, RetryPolicy = None };

        //    await Assert.ThrowsExceptionAsync<InvalidOperationException>(
        //        () => step.ExecuteAsync());
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_MissingTimeout_Throws()
        //{
        //    var step = new RequestStep()
        //    { Method = "GET", Url = "http://test.html", PayloadSize = 15, Retries = 1, RetryPolicy = None };

        //    await Assert.ThrowsExceptionAsync<InvalidOperationException>(
        //        () => step.ExecuteAsync());
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_InvalidRetries_Throws()
        //{
        //    var step = new RequestStep()
        //    { Method = "GET", Url = "http://test.html", PayloadSize = 15, Timeout = 1, Retries = -1, RetryPolicy = None };

        //    await Assert.ThrowsExceptionAsync<InvalidOperationException>(
        //        () => step.ExecuteAsync());
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_MissingRetries_Throws()
        //{
        //    var step = new RequestStep()
        //    { Method = "GET", Url = "http://test.html", PayloadSize = 15, Timeout = 1, RetryPolicy = None };

        //    await Assert.ThrowsExceptionAsync<InvalidOperationException>(
        //        () => step.ExecuteAsync());
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_MissingRetryPolicy_Throws()
        //{
        //    var step = new RequestStep()
        //    { Method = "GET", Url = "http://test.html", PayloadSize = 15, Timeout = 1, Retries = 1 };

        //    await Assert.ThrowsExceptionAsync<InvalidOperationException>(
        //        () => step.ExecuteAsync());
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_MissingMethod_Throws()
        //{
        //    var step = new RequestStep()
        //    { Url = "http://test.html", PayloadSize = 15, Timeout = 1, Retries = 1, RetryPolicy = None };

        //    await Assert.ThrowsExceptionAsync<InvalidOperationException>(
        //        () => step.ExecuteAsync());
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_UnknownMethod_Throws()
        //{
        //    var step = new RequestStep()
        //    { Method = "UNKNOWN", Url = "http://test.html", PayloadSize = 15, Timeout = 1, Retries = 1, RetryPolicy = None };

        //    await Assert.ThrowsExceptionAsync<InvalidOperationException>(
        //        () => step.ExecuteAsync());
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_AbsoluteUrlSuccess_ReturnsSuccess()
        //{
        //    // TODO: mock endpoint
        //    var step = new RequestStep()
        //    { Method = "GET", Url = "http://test.html", PayloadSize = 15, Timeout = 0, Retries = 0, RetryPolicy = None };

        //    ExecutionStatus status = await step.ExecuteAsync();
        //    Assert.AreEqual(ExecutionStatus.Success, status);
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_AbsoluteUrlError_ReturnsFail()
        //{
        //    // TODO: mock endpoint
        //    var step = new RequestStep()
        //    { Method = "GET", Url = "http://test.html", PayloadSize = 15, Timeout = 0, Retries = 0, RetryPolicy = None };

        //    ExecutionStatus status = await step.ExecuteAsync();
        //    Assert.AreEqual(ExecutionStatus.Fail, status);
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_MethodDelete_CallsDelete()
        //{
        //    // TODO: mock endpoint
        //    var step = new RequestStep()
        //    { Method = "DELETE", Url = "http://test/name", PayloadSize = 15, Timeout = 0, Retries = 0, RetryPolicy = None };

        //    ExecutionStatus status = await step.ExecuteAsync();
        //    Assert.AreEqual(ExecutionStatus.Success, status);
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_MethodHead_CallsHead()
        //{
        //    // TODO: mock endpoint
        //    var step = new RequestStep()
        //    { Method = "HEAD", Url = "http://test/name", PayloadSize = 15, Timeout = 0, Retries = 0, RetryPolicy = None };

        //    ExecutionStatus status = await step.ExecuteAsync();
        //    Assert.AreEqual(ExecutionStatus.Success, status);
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_MethodOptions_CallsOptions()
        //{
        //    // TODO: mock endpoint
        //    var step = new RequestStep()
        //    { Method = "OPTIONS", Url = "http://test/name", PayloadSize = 15, Timeout = 0, Retries = 0, RetryPolicy = None };

        //    ExecutionStatus status = await step.ExecuteAsync();
        //    Assert.AreEqual(ExecutionStatus.Success, status);
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_MethodPost_CallsPost()
        //{
        //    // TODO: mock endpoint
        //    var step = new RequestStep()
        //    { Method = "post", Url = "http://test/name", PayloadSize = 15, Timeout = 0, Retries = 0, RetryPolicy = None };

        //    ExecutionStatus status = await step.ExecuteAsync();
        //    Assert.AreEqual(ExecutionStatus.Success, status);
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_MethodPut_CallsPut()
        //{
        //    // TODO: mock endpoint
        //    var step = new RequestStep()
        //    { Method = "PUT", Url = "http://test/name", PayloadSize = 15, Timeout = 0, Retries = 0, RetryPolicy = None };

        //    ExecutionStatus status = await step.ExecuteAsync();
        //    Assert.AreEqual(ExecutionStatus.Success, status);
        //}


        //[TestMethod]
        //public async Task ExecuteAsync_MethodTrace_CallsTrace()
        //{
        //    // TODO: mock endpoint
        //    var step = new RequestStep()
        //    { Method = "TRACE", Url = "http://test/name", PayloadSize = 15, Timeout = 0, Retries = 0, RetryPolicy = None };

        //    ExecutionStatus status = await step.ExecuteAsync();
        //    Assert.AreEqual(ExecutionStatus.Success, status);
        //}


        // TODO: add UTs with complex timeout and retry policies
    }
 }
