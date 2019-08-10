using CoreService.Simulation.Steps;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Contrib.HttpClient;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.Net;
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
            client.BaseAddress = baseUri;

            handler.SetupRequest(HttpMethod.Get, $"{baseUri}test/")
                .Throws(new Exception("test exception"));

            var factory = handler.CreateClientFactory();

            Mock.Get(factory)
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() => client);

            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };
            step.Configure(factory);

            var result = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.Success, result);
            client.Dispose();
        }


        [TestMethod]
        public async Task ExecuteAsync_ReuseClient_Asynchronous_ResponseOk_ReturnsSuccess()
        {
            Uri baseUri = new Uri("http://test.com/");
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();
            client.BaseAddress = baseUri;
            handler.SetupRequest(HttpMethod.Head, $"{baseUri}test/")
                .ReturnsResponse(HttpStatusCode.OK);

            var factory = handler.CreateClientFactory();
            Mock.Get(factory)
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() => client);

            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "HEAD", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };
            step.Configure(factory);

            var result = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.Success, result);
            client.Dispose();
        }


        [TestMethod]
        public async Task ExecuteAsync_ReuseHttpClient_ExceptionThrown_Throws()
        {
            Uri baseUri = new Uri("http://test.com/");
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();
            client.BaseAddress = baseUri;
            handler.SetupRequest(HttpMethod.Delete, $"{baseUri}test/")
                .Throws(new Exception("test exception"));

            var factory = handler.CreateClientFactory();
            Mock.Get(factory)
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() => client);

            var step = new RequestStep()
            { Asynchrounous = false, ClientName = "testClient", Method = "DELETE", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };
            step.Configure(factory);

            await Assert.ThrowsExceptionAsync<Exception>(
                () => step.ExecuteAsync());
            client.Dispose();
        }


        [TestMethod]
        public async Task ExecuteAsync_ReuseHttpClient_ResponseOk_ReturnsSuccess()
        {
            Uri baseUri = new Uri("http://test.com/");
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();
            client.BaseAddress = baseUri;
            handler.SetupRequest(HttpMethod.Options, $"{baseUri}test/")
                .ReturnsResponse(HttpStatusCode.OK);

            var factory = handler.CreateClientFactory();
            Mock.Get(factory)
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() => client);

            var step = new RequestStep()
            { Asynchrounous = false, ClientName = "testClient", Method = "OPTIONS", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };
            step.Configure(factory);
            var result = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.Success, result);
            client.Dispose();
        }


        [TestMethod]
        public async Task ExecuteAsync_ReuseHttpClient_ResponseNotOk_ReturnsFail()
        {
            Uri baseUri = new Uri("http://test.com/");
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();
            client.BaseAddress = baseUri;
            handler.SetupRequest(HttpMethod.Trace, $"{baseUri}test/")
                .ReturnsResponse(HttpStatusCode.BadRequest);

            var factory = handler.CreateClientFactory();
            Mock.Get(factory)
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() => client);

            var step = new RequestStep()
            { Asynchrounous = false, ClientName = "testClient", Method = "TRACE", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };
            step.Configure(factory);
            var result = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.Fail, result);
            client.Dispose();
        }


        [TestMethod]
        public async Task ExecuteAsync_Asynchronous_TaskFaulted_ReturnsSuccess()
        {
            Uri baseUri = new Uri("http://test.com/");
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();
            client.BaseAddress = baseUri;

            handler.SetupRequest(HttpMethod.Get, $"{baseUri}test/")
                .Throws(new Exception("test exception"));

            var factory = handler.CreateClientFactory();
            var policies = Policy.Wrap(Policy.NoOpAsync(), Policy.NoOpAsync());

            Mock.Get(factory)
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() => client);

            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };
            step.Configure(factory, policies);

            var result = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.Success, result);

            // Will already be disposed but non-breaking call suppresses warning
            client.Dispose();
        }


        [TestMethod]
        public async Task ExecuteAsync_Asynchronous_ResponseOk_ReturnsSuccess()
        {
            Uri baseUri = new Uri("http://test.com/");
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();
            client.BaseAddress = baseUri;

            handler.SetupRequest(HttpMethod.Post, $"{baseUri}test/")
                .ReturnsResponse(HttpStatusCode.OK);

            var factory = handler.CreateClientFactory();
            var policies = Policy.Wrap(Policy.NoOpAsync(), Policy.NoOpAsync());

            Mock.Get(factory)
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() => client);

            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "POST", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };
            step.Configure(factory, policies);

            var result = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.Success, result);

            // Will already be disposed but non-breaking call suppresses warning
            client.Dispose();
        }


        [TestMethod]
        public async Task ExecuteAsync_Asynchronous_ResponseNotOk_ReturnsSuccess()
        {
            Uri baseUri = new Uri("http://test.com/");
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();
            client.BaseAddress = baseUri;

            handler.SetupRequest(HttpMethod.Get, $"{baseUri}test/")
                .ReturnsResponse(HttpStatusCode.BadRequest);

            var factory = handler.CreateClientFactory();
            var policies = Policy.Wrap(Policy.NoOpAsync(), Policy.NoOpAsync());

            Mock.Get(factory)
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() => client);

            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };
            step.Configure(factory, policies);

            var result = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.Success, result);

            // Will already be disposed but non-breaking call suppresses warning
            client.Dispose();
        }


        [TestMethod]
        public async Task ExecuteAsync_ThrowsException_Throws()
        {
            Uri baseUri = new Uri("http://test.com/");
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();
            client.BaseAddress = baseUri;

            handler.SetupRequest(HttpMethod.Get, $"{baseUri}test/")
                .Throws(new Exception("test exception"));

            var factory = handler.CreateClientFactory();
            var policies = Policy.Wrap(Policy.NoOpAsync(), Policy.NoOpAsync());

            Mock.Get(factory)
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() => client);

            var step = new RequestStep()
            { Asynchrounous = false, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };
            step.Configure(factory, policies);

            await Assert.ThrowsExceptionAsync<Exception>(
                () => step.ExecuteAsync());

            // Will already be disposed but non-breaking call suppresses warning
            client.Dispose();
        }


        [TestMethod]
        public async Task ExecuteAsync_ResponseOk_ReturnsSuccess()
        {
            Uri baseUri = new Uri("http://test.com/");
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();
            client.BaseAddress = baseUri;
            handler.SetupRequest(HttpMethod.Put, $"{baseUri}test/")
                .ReturnsResponse(HttpStatusCode.OK);

            var factory = handler.CreateClientFactory();
            var policies = Policy.Wrap(Policy.NoOpAsync(), Policy.NoOpAsync());

            Mock.Get(factory)
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() => client);

            var step = new RequestStep()
            { Asynchrounous = false, ClientName = "testClient", Method = "PUT", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };
            step.Configure(factory, policies);
            var result = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.Success, result);

            // Will already be disposed but non-breaking call suppresses warning
            client.Dispose();
        }


        [TestMethod]
        public async Task ExecuteAsync_ResponseNotOk_ReturnsFail()
        {
            Uri baseUri = new Uri("http://test.com/");
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();
            client.BaseAddress = baseUri;
            handler.SetupRequest(HttpMethod.Options, $"{baseUri}test/")
                .ReturnsResponse(HttpStatusCode.BadRequest);

            var factory = handler.CreateClientFactory();
            var policies = Policy.Wrap(Policy.NoOpAsync(), Policy.NoOpAsync());

            Mock.Get(factory)
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() => client);

            var step = new RequestStep()
            { Asynchrounous = false, ClientName = "testClient", Method = "OPTIONS", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };
            step.Configure(factory, policies);
            var result = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.Fail, result);

            // Will already be disposed but non-breaking call suppresses warning
            client.Dispose();
        }


        [TestMethod]
        public async Task ExecuteAsync_PoliciesNull_ResponseOk_ReturnsSuccess()
        {
            Uri baseUri = new Uri("http://test.com/");
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();
            client.BaseAddress = baseUri;
            handler.SetupRequest(HttpMethod.Options, $"{baseUri}test/")
                .ReturnsResponse(HttpStatusCode.OK);

            var factory = handler.CreateClientFactory();

            Mock.Get(factory)
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() => client);

            var step = new RequestStep()
            { Asynchrounous = false, ClientName = "testClient", Method = "OPTIONS", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };
            step.Configure(factory, null);
            var result = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.Success, result);

            // Will already be disposed but non-breaking call suppresses warning
            client.Dispose();
        }


        [TestMethod]
        public async Task ExecuteAsync_PoliciesNull_Asynchronous_TaskFaulted_ReturnsSuccess()
        {
            Uri baseUri = new Uri("http://test.com/");
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var client = handler.CreateClient();
            client.BaseAddress = baseUri;

            handler.SetupRequest(HttpMethod.Get, $"{baseUri}test/")
                .Throws(new Exception("test exception"));

            var factory = handler.CreateClientFactory();

            Mock.Get(factory)
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() => client);

            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };
            step.Configure(factory, null);

            var result = await step.ExecuteAsync();

            Assert.AreEqual(ExecutionStatus.Success, result);

            // Will already be disposed but non-breaking call suppresses warning
            client.Dispose();
        }


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
            var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var policies = Policy.Wrap(Policy.NoOp(), Policy.NoOp());

            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = true };

            Assert.ThrowsException<InvalidOperationException>(
                () => step.Configure(factory.Object, policies));
        }


        [TestMethod]
        public void Configure_Local_FactoryNull_Throws()
        {
            var policies = Policy.Wrap(Policy.NoOp(), Policy.NoOp());

            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };

            Assert.ThrowsException<ArgumentNullException>(
                () => step.Configure(null, policies));
        }


        [TestMethod]
        public void Configure_Local_PoliciesNull_SetsConfigured()
        {
            var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);

            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };

            step.Configure(factory.Object, null);
            Assert.IsTrue(step.Configured);
        }


        [TestMethod]
        public void Configure_Local_AllValuesProvided_SetsConfigured()
        {
            var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var policies = Policy.Wrap(Policy.NoOp(), Policy.NoOp());

            var step = new RequestStep()
            { Asynchrounous = true, ClientName = "testClient", Method = "GET", Path = "test/", PayloadSize = 16, ReuseHttpClient = false };

            step.Configure(factory.Object, policies);
            Assert.IsTrue(step.Configured);
        }
    }
}
