using ClusterEmulator.ApplicationInsights.Telemetry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;

namespace ClusterEmulator.ApplicationInsights.Test.Telemetry
{
    [TestClass]
    public class OperationIdEnricherUnitTests
    {
        [TestMethod]
        public void Enrich_WithNullEvent_ThrowsException()
        {
            var factory = new Mock<ILogEventPropertyFactory>(MockBehavior.Strict);
            var enricher = new OperationIdEnricher();

            Assert.ThrowsException<ArgumentNullException>(
                () => enricher.Enrich(null, factory.Object));
        }


        [TestMethod]
        public void Enrich_WithNullFactory_MissingRequestId_ProcessesCorrectly()
        {
            var messageTemplate = new MessageTemplate("test", new List<MessageTemplateToken>());
            var logEvent = new LogEvent(DateTime.UtcNow, LogEventLevel.Verbose, null, messageTemplate, new List<LogEventProperty>());
            var enricher = new OperationIdEnricher();

            enricher.Enrich(logEvent, null);

            Assert.IsFalse(logEvent.Properties.ContainsKey(PropertyNames.OperationId));
        }


        [TestMethod]
        public void Enrich_WithValidRequestId_ProcessesCorrectly()
        {
            var factory = new Mock<ILogEventPropertyFactory>(MockBehavior.Strict);
            var messageTemplate = new MessageTemplate("test", new List<MessageTemplateToken>());
            var logEventProperty = new LogEventProperty(PropertyNames.RequestId, new ScalarValue("test"));
            var logEvent = new LogEvent(DateTime.UtcNow, LogEventLevel.Verbose, null, messageTemplate, new List<LogEventProperty> { logEventProperty });
            var enricher = new OperationIdEnricher();

            enricher.Enrich(logEvent, factory.Object);

            Assert.IsTrue(logEvent.Properties.ContainsKey(PropertyNames.OperationId));
        }
    }
}
