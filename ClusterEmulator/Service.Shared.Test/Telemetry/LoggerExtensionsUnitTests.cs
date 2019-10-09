using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using System;
using LoggerExtensions = ClusterEmulator.Service.Shared.Telemetry.LoggerExtensions;

namespace ClusterEmulator.Service.Shared.Test.Telemetry
{
    [TestClass]
    public class LoggerExtensionsUnitTests
    {
        [TestMethod]
        public void WithOperationId_NullConfiguration_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => LoggerExtensions.WithOperationId(null));
        }


        [TestMethod]
        public void WithOperationId_ValidConfiguration_ReturnsLoggerConfiguration()
        {
            // Arrange
            var enrichmentConfiguration = new LoggerConfiguration().Enrich;

            // Act
            var loggerConfiguration = LoggerExtensions.WithOperationId(enrichmentConfiguration);

            // Verify
            Assert.IsNotNull(loggerConfiguration);
        }
    }
}
