using CoreService.Simulation.Steps;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoreService.Test.Simulation.Steps
{
    [TestClass]
    public class RequestStepUnitTests
    {
        [TestMethod]
        public void Deserialization_ValidData_CreatesValidInstance()
        {
            RequestStep step = JsonConvert.DeserializeObject<RequestStep>(
                "{ async : true, client : 'testClient', method : 'get', path : 'test/', size : 128, reuseSockets : true }");

            Assert.IsTrue(step.Async);
            Assert.AreEqual("testClient", step.ClientName);
            Assert.AreEqual("get", step.Method);
            Assert.AreEqual("test/", step.Path);
            Assert.AreEqual(128, step.PayloadSize);
            Assert.AreEqual(true, step.ReuseHttpClient);
        }


        [TestMethod]
        public void Deserialization_InvalidData_Throws()
        {
            Assert.ThrowsException<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<RequestStep>("{ }"));
        }


        [TestMethod]
        public async Task ExecuteAsync_MissingClientName_Throws()
        {
            var step = new RequestStep()
            { Async = true, ClientName = string.Empty, Method = "GET", Path = "test/", PayloadSize = 15, ReuseHttpClient = true };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_MissingPath_Throws()
        {
            var step = new RequestStep()
            { Async = true, ClientName = "testClient", Method = "GET", Path = string.Empty, PayloadSize = 15, ReuseHttpClient = true };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_FilePath_Throws()
        {
            var step = new RequestStep()
            { Async = true, ClientName = "testClient", Method = "GET", Path = "file://test.txt", PayloadSize = 15, ReuseHttpClient = true };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_AbsoluteUrl_Throws()
        {
            var step = new RequestStep()
            { Async = true, ClientName = "testClient", Method = "GET", Path = "http://test.html", PayloadSize = 15, ReuseHttpClient = true };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_MissingMethod_Throws()
        {
            var step = new RequestStep()
            { Async = true, ClientName = "testClient", Method = string.Empty, Path = "test/", PayloadSize = 15, ReuseHttpClient = true };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_UnsupportedMethod_Throws()
        {
            var step = new RequestStep()
            { Async = true, ClientName = "testClient", Method = "BREAK", Path = "test/", PayloadSize = 15, ReuseHttpClient = true };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_InvalidPayloadSize_Throws()
        {
            var step = new RequestStep()
            { Async = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = -1, ReuseHttpClient = true };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_NotConfigured_Throws()
        {
            var step = new RequestStep()
            { Async = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_ReuseHttpClient_CreateClientReturnsNull_Throws()
        {
            Mock<IHttpClientFactory> factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            // TODO: factory.Setup(f => f.CreateClient()).Returns<HttpClient>(null);

            var step = new RequestStep()
            { Async = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };
            step.Configure(factory.Object);

            await Assert.ThrowsExceptionAsync<NullReferenceException>(
                () => step.ExecuteAsync());
        }


        // TODO: complete ExecuteAsync


        [TestMethod]
        public void Configure_Factory_ReuseHttpClientFalse_Throws()
        {
            Mock<IHttpClientFactory> factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var step = new RequestStep()
            { Async = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };

            Assert.ThrowsException<InvalidOperationException>(
                () => step.Configure(factory.Object));
        }


        [TestMethod]
        public void Configure_Factory_NullFactory_Throws()
        {
            var step = new RequestStep()
            { Async = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };

            Assert.ThrowsException<ArgumentNullException>(
                () => step.Configure(null));
        }


        [TestMethod]
        public void Configure_Factory_ValidFactory_SetsConfigured()
        {
            Mock<IHttpClientFactory> factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var step = new RequestStep()
            { Async = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };

            step.Configure(factory.Object);

            Assert.IsTrue(step.Configured);
        }


        [TestMethod]
        public void Configure_Factory_Twice_Throws()
        {
            Mock<IHttpClientFactory> factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var step = new RequestStep()
            { Async = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };

            step.Configure(factory.Object);

            Assert.IsTrue(step.Configured);
            Assert.ThrowsException<InvalidOperationException>(
                () => step.Configure(factory.Object));
        }


        [TestMethod]
        public void Configure_Local_ReuseHttpClientTrue_Throws()
        {
            var policies = Policy.Wrap(Policy.NoOp(), Policy.NoOp());
            var headers = new Dictionary<string, string>();

            var step = new RequestStep()
            { Async = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };

            Assert.ThrowsException<InvalidOperationException>(
                () => step.Configure(policies, "http://test.com/", headers));
        }


        [TestMethod]
        public void Configure_Local_BaseAddressEmpty_Throws()
        {
            var policies = Policy.Wrap(Policy.NoOp(), Policy.NoOp());
            var headers = new Dictionary<string, string>();

            var step = new RequestStep()
            { Async = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };

            Assert.ThrowsException<ArgumentNullException>(
                () => step.Configure(policies, string.Empty, headers));
        }


        [TestMethod]
        public void Configure_Local_BaseAddressRelative_Throws()
        {
            var policies = Policy.Wrap(Policy.NoOp(), Policy.NoOp());
            var headers = new Dictionary<string, string>();

            var step = new RequestStep()
            { Async = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };

            Assert.ThrowsException<ArgumentException>(
                () => step.Configure(policies, "/test/", headers));
        }


        [TestMethod]
        public void Configure_Local_PoliciesNull_Throws()
        {
            var headers = new Dictionary<string, string>();

            var step = new RequestStep()
            { Async = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };

            Assert.ThrowsException<ArgumentNullException>(
                () => step.Configure(null, "http://test.com/", headers));
        }


        [TestMethod]
        public void Configure_Local_HeadersNull_SetsConfigured()
        {
            var policies = Policy.Wrap(Policy.NoOp(), Policy.NoOp());

            var step = new RequestStep()
            { Async = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };

            step.Configure(policies, "http://test.com/", null);
            Assert.IsTrue(step.Configured);
        }


        [TestMethod]
        public void Configure_Local_AllValuesProvided_SetsConfigured()
        {
            var policies = Policy.Wrap(Policy.NoOp(), Policy.NoOp());
            var headers = new Dictionary<string, string>();

            var step = new RequestStep()
            { Async = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };

            step.Configure(policies, "http://test.com/", headers);
            Assert.IsTrue(step.Configured);
        }
    }
}
