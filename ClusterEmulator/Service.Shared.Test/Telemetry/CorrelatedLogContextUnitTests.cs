using ClusterEmulator.Service.Shared.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace ClusterEmulator.Service.Shared.Test.Telemetry
{
    [TestClass]
    public class CorrelatedLogContextUnitTests
    {
        [TestMethod]
        public void Create_NullContext_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => CorrelatedLogContext.Create(null));
        }


        [TestMethod]
        public void Create_NullRequest_Throws()
        {
            // Arrange
            var httpContext = new Mock<HttpContext>(MockBehavior.Strict);
            httpContext.Setup(c => c.Request)
                .Returns<HttpRequest>(null);

            // Act & Verify
            Assert.ThrowsException<ArgumentException>(
                () => CorrelatedLogContext.Create(httpContext.Object));
        }


        [TestMethod]
        public void Create_NullHeaders_Throws()
        {
            // Arrange
            var httpRequest = new Mock<HttpRequest>(MockBehavior.Strict);
            httpRequest.Setup(r => r.Headers)
                .Returns<IHeaderDictionary>(null);
            var httpContext = new Mock<HttpContext>(MockBehavior.Strict);
            httpContext.Setup(c => c.Request)
                .Returns(httpRequest.Object);

            // Act & Verify
            Assert.ThrowsException<ArgumentException>(
                () => CorrelatedLogContext.Create(httpContext.Object));
        }


        [TestMethod]
        public void Create_HeaderNotFound_ReturnsDisposable()
        {
            // Arrange
            StringValues values = new StringValues("test");
            var headers = new Mock<IHeaderDictionary>(MockBehavior.Strict);
            headers.Setup(h => h.TryGetValue(It.IsAny<string>(), out values))
                .Returns(false);
            var httpRequest = new Mock<HttpRequest>(MockBehavior.Strict);
            httpRequest.Setup(r => r.Headers)
                .Returns(headers.Object);
            var httpContext = new Mock<HttpContext>(MockBehavior.Strict);
            httpContext.Setup(c => c.Request)
                .Returns(httpRequest.Object);

            // Act
            IDisposable disposable = CorrelatedLogContext.Create(httpContext.Object);

            // Verify
            Assert.IsNotNull(disposable);
            disposable.Dispose();
        }


        [TestMethod]
        public void Create_HeaderBlank_ReturnsDisposable()
        {
            // Arrange
            StringValues values = new StringValues(string.Empty);
            var headers = new Mock<IHeaderDictionary>(MockBehavior.Strict);
            headers.Setup(h => h.TryGetValue(It.IsAny<string>(), out values))
                .Returns(true);
            var httpRequest = new Mock<HttpRequest>(MockBehavior.Strict);
            httpRequest.Setup(r => r.Headers)
                .Returns(headers.Object);
            var httpContext = new Mock<HttpContext>(MockBehavior.Strict);
            httpContext.Setup(c => c.Request)
                .Returns(httpRequest.Object);

            // Act
            IDisposable disposable = CorrelatedLogContext.Create(httpContext.Object);

            // Verify
            Assert.IsNotNull(disposable);
            disposable.Dispose();
        }


        [TestMethod]
        public void Create_HeaderEmpty_ReturnsDisposable()
        {
            // Arrange
            StringValues values = new StringValues();
            var headers = new Mock<IHeaderDictionary>(MockBehavior.Strict);
            headers.Setup(h => h.TryGetValue(It.IsAny<string>(), out values))
                .Returns(true);
            var httpRequest = new Mock<HttpRequest>(MockBehavior.Strict);
            httpRequest.Setup(r => r.Headers)
                .Returns(headers.Object);
            var httpContext = new Mock<HttpContext>(MockBehavior.Strict);
            httpContext.Setup(c => c.Request)
                .Returns(httpRequest.Object);

            // Act
            IDisposable disposable = CorrelatedLogContext.Create(httpContext.Object);

            // Verify
            Assert.IsNotNull(disposable);
            disposable.Dispose();
        }


        [TestMethod]
        public void Create_HeaderCorrect_ReturnsDisposable()
        {
            // Arrange
            StringValues values = new StringValues("test");
            var headers = new Mock<IHeaderDictionary>(MockBehavior.Strict);
            headers.Setup(h => h.TryGetValue(It.IsAny<string>(), out values))
                .Returns(true);
            var httpRequest = new Mock<HttpRequest>(MockBehavior.Strict);
            httpRequest.Setup(r => r.Headers)
                .Returns(headers.Object);
            var httpContext = new Mock<HttpContext>(MockBehavior.Strict);
            httpContext.Setup(c => c.Request)
                .Returns(httpRequest.Object);

            // Act
            IDisposable disposable = CorrelatedLogContext.Create(httpContext.Object);

            // Verify
            Assert.IsNotNull(disposable);
            disposable.Dispose();
        }
    }
}
