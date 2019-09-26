using ClusterEmulator.Service.Shared.Telemetry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using ServiceFabric.Mocks;
using System;
using System.Collections.Generic;

namespace ClusterEmulator.Service.Shared.Test.Telemetry
{
    [TestClass]
    public class StatelessServiceEnricherUnitTests
    {
        [TestMethod]
        public void Constructor_WithNullContext_ThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new StatelessServiceEnricher(null));
        }


        [TestMethod]
        public void Enrich_WithNullEvent_ThrowsException()
        {
            // Arrange
            var context = MockStatelessServiceContextFactory.Default;
            var enricher = new StatelessServiceEnricher(context);
            var propertyFactory = new Mock<ILogEventPropertyFactory>(MockBehavior.Strict);

            // Act & Verify
            Assert.ThrowsException<ArgumentNullException>(
                () => enricher.Enrich(null, propertyFactory.Object));
        }


        [TestMethod]
        public void Enrich_WithNullFactory_ThrowsException()
        {
            // Arrange
            var context = MockStatelessServiceContextFactory.Default;
            var enricher = new StatelessServiceEnricher(context);
            var logEvent = new LogEvent(
                timestamp: DateTimeOffset.Now,
                level: LogEventLevel.Information,
                exception: null,
                messageTemplate: new MessageTemplate("test", new List<MessageTemplateToken>()),
                properties: new List<LogEventProperty>());

            // Act & Verify
            Assert.ThrowsException<ArgumentNullException>(
                () => enricher.Enrich(logEvent, null));
        }


        [TestMethod]
        public void Enrich_WithValidState_SetsPropertiesCorrectly()
        {
            // Arrange
            var context = MockStatelessServiceContextFactory.Default;
            var enricher = new StatelessServiceEnricher(context);
            var propertyFactory = new Mock<ILogEventPropertyFactory>(MockBehavior.Strict);
            var logEvent = new LogEvent(
                timestamp: DateTimeOffset.Now,
                level: LogEventLevel.Information,
                exception: null,
                messageTemplate: new MessageTemplate("test", new List<MessageTemplateToken>()),
                properties: new List<LogEventProperty>());

            propertyFactory.Setup(pf => pf.CreateProperty(It.IsAny<string>(), It.IsAny<object>(), false))
                .Returns(new LogEventProperty("test", new ScalarValue("test")));

            // Act
            enricher.Enrich(logEvent, propertyFactory.Object);

            // Verify
            propertyFactory.Verify(
                pf => pf.CreateProperty("ServiceTypeName", It.IsAny<string>(), false), Times.Once);
            propertyFactory.Verify(
                pf => pf.CreateProperty("ServiceName", It.IsAny<Uri>(), false), Times.Once);
            propertyFactory.Verify(
                pf => pf.CreateProperty("PartitionId", It.IsAny<Guid>(), false), Times.Once);
            propertyFactory.Verify(
                pf => pf.CreateProperty("InstanceId", It.IsAny<long>(), false), Times.Once);
            propertyFactory.Verify(
                pf => pf.CreateProperty("NodeName", It.IsAny<string>(), false), Times.Once);
        }
    }
}
