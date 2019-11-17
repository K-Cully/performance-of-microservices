using EmulationService.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace EmulationService.Test.Telemetry
{
    [TestClass]
    public class CorrelatedLogContextUnitTests
    {
        [TestMethod]
        public void Create_NullContext_Throws()
        {
            var context = new CorrelatedLogContext();
            Assert.ThrowsException<ArgumentNullException>(
                () => context.InitializeFrom(null));
        }


        [TestMethod]
        public void Create_NullRequest_Throws()
        {
            // Arrange
            var context = new CorrelatedLogContext();
            var httpContext = new Mock<HttpContext>(MockBehavior.Strict);
            httpContext.Setup(c => c.Request)
                .Returns<HttpRequest>(null);

            // Act & Verify
            Assert.ThrowsException<ArgumentException>(
                () => context.InitializeFrom(httpContext.Object));
        }


        [TestMethod]
        public void Create_NullHeaders_Throws()
        {
            // Arrange
            var context = new CorrelatedLogContext();
            var httpRequest = new Mock<HttpRequest>(MockBehavior.Strict);
            httpRequest.Setup(r => r.Headers)
                .Returns<IHeaderDictionary>(null);
            var httpContext = new Mock<HttpContext>(MockBehavior.Strict);
            httpContext.Setup(c => c.Request)
                .Returns(httpRequest.Object);

            // Act & Verify
            Assert.ThrowsException<ArgumentException>(
                () => context.InitializeFrom(httpContext.Object));
        }


        [TestMethod]
        public void Create_HeaderNotFound_ReturnsDisposable()
        {
            // Arrange
            var context = new CorrelatedLogContext();
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
            IDisposable disposable = context.InitializeFrom(httpContext.Object);

            // Verify
            Assert.IsNotNull(disposable);
            disposable.Dispose();
        }


        [TestMethod]
        public void Create_HeaderBlank_ReturnsDisposable()
        {
            // Arrange
            var context = new CorrelatedLogContext();
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
            IDisposable disposable = context.InitializeFrom(httpContext.Object);

            // Verify
            Assert.IsNotNull(disposable);
            disposable.Dispose();
        }


        [TestMethod]
        public void Create_HeaderEmpty_ReturnsDisposable()
        {
            // Arrange
            var context = new CorrelatedLogContext();
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
            IDisposable disposable = context.InitializeFrom(httpContext.Object);

            // Verify
            Assert.IsNotNull(disposable);
            disposable.Dispose();
        }


        [TestMethod]
        public void Create_HeaderCorrect_ReturnsDisposable()
        {
            // Arrange
            var context = new CorrelatedLogContext();
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
            IDisposable disposable = context.InitializeFrom(httpContext.Object);

            // Verify
            Assert.IsNotNull(disposable);
            disposable.Dispose();
        }
    }
}
