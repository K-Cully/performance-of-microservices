using CoreService.Simulation.Steps;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Contrib.HttpClient;
using Moq.Protected;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
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

            Assert.IsTrue(step.Asynchrounous);
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
            { Asynchrounous = true, ClientName = string.Empty, Method = "GET", Path = "test/", PayloadSize = 15, ReuseHttpClient = true };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_MissingPath_Throws()
        {
            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = string.Empty, PayloadSize = 15, ReuseHttpClient = true };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_FilePath_Throws()
        {
            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "file://test.txt", PayloadSize = 15, ReuseHttpClient = true };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_AbsoluteUrl_Throws()
        {
            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "http://test.html", PayloadSize = 15, ReuseHttpClient = true };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_MissingMethod_Throws()
        {
            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = string.Empty, Path = "test/", PayloadSize = 15, ReuseHttpClient = true };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_UnsupportedMethod_Throws()
        {
            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "BREAK", Path = "test/", PayloadSize = 15, ReuseHttpClient = true };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_InvalidPayloadSize_Throws()
        {
            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = -1, ReuseHttpClient = true };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_NotConfigured_Throws()
        {
            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_ReuseClient_ClientNull_ReturnsUnexpected()
        {
            var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            factory.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns<HttpClient>(null);

            var step = new RequestStep()
            { Asynchrounous = false, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };
            step.Configure(factory.Object);

            Assert.AreEqual(ExecutionStatus.Unexpected, await step.ExecuteAsync());
        }


        [TestMethod]
        public async Task ExecuteAsync_ReuseClient_Asynchronous_TaskFaulted_ReturnsSuccess()
        {
            Uri baseUri = new Uri("http://test.com/");
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();

            // TODO: return correct response
            handler.SetupRequest(HttpMethod.Get, $"{baseUri}/api/stuff")
                .ReturnsResponse("stuff");

            var factory = handler.CreateClientFactory();

            Mock.Get(factory)
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() => client);

            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };
            step.Configure(factory);

            var result = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.Success, result);
        }


        [TestMethod]
        public async Task ExecuteAsync_ReuseHttpClient_CreateClientReturnsClient_Throws()
        {
            // TODO: rename and update
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = new HttpClient(handler.Object, false);
            handler.Protected().As<IHttpMessageHandler>()
                .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()));

            var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            factory.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(client);

            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };
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
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };

            Assert.ThrowsException<InvalidOperationException>(
                () => step.Configure(factory.Object));
        }


        [TestMethod]
        public void Configure_Factory_NullFactory_Throws()
        {
            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };

            Assert.ThrowsException<ArgumentNullException>(
                () => step.Configure(null));
        }


        [TestMethod]
        public void Configure_Factory_ValidFactory_SetsConfigured()
        {
            Mock<IHttpClientFactory> factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };

            step.Configure(factory.Object);

            Assert.IsTrue(step.Configured);
        }


        [TestMethod]
        public void Configure_Factory_Twice_Throws()
        {
            Mock<IHttpClientFactory> factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };

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
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };

            Assert.ThrowsException<InvalidOperationException>(
                () => step.Configure(policies, "http://test.com/", headers));
        }


        [TestMethod]
        public void Configure_Local_BaseAddressEmpty_Throws()
        {
            var policies = Policy.Wrap(Policy.NoOp(), Policy.NoOp());
            var headers = new Dictionary<string, string>();

            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };

            Assert.ThrowsException<ArgumentNullException>(
                () => step.Configure(policies, string.Empty, headers));
        }


        [TestMethod]
        public void Configure_Local_BaseAddressRelative_Throws()
        {
            var policies = Policy.Wrap(Policy.NoOp(), Policy.NoOp());
            var headers = new Dictionary<string, string>();

            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };

            Assert.ThrowsException<ArgumentException>(
                () => step.Configure(policies, "/test/", headers));
        }


        [TestMethod]
        public void Configure_Local_PoliciesNull_Throws()
        {
            var headers = new Dictionary<string, string>();

            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };

            Assert.ThrowsException<ArgumentNullException>(
                () => step.Configure(null, "http://test.com/", headers));
        }


        [TestMethod]
        public void Configure_Local_HeadersNull_SetsConfigured()
        {
            var policies = Policy.Wrap(Policy.NoOp(), Policy.NoOp());

            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };

            step.Configure(policies, "http://test.com/", null);
            Assert.IsTrue(step.Configured);
        }


        [TestMethod]
        public void Configure_Local_AllValuesProvided_SetsConfigured()
        {
            var policies = Policy.Wrap(Policy.NoOp(), Policy.NoOp());
            var headers = new Dictionary<string, string>();

            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };

            step.Configure(policies, "http://test.com/", headers);
            Assert.IsTrue(step.Configured);
        }
    }
}
