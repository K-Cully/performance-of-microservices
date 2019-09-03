using CoreService.Telemetry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CoreService.Test.Telemetry
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
        public void Enrich_WithNullFactoryAndNullActivity_ProcessesCorrectly()
        {
            var messageTemplate = new MessageTemplate("test", new List<MessageTemplateToken>());
            var logEvent = new LogEvent(DateTime.UtcNow, LogEventLevel.Verbose, null, messageTemplate, new List<LogEventProperty>());
            var enricher = new OperationIdEnricher();

            enricher.Enrich(logEvent, null);

            Assert.IsFalse(logEvent.Properties.ContainsKey("Operation Id"));
            Assert.IsFalse(logEvent.Properties.ContainsKey("Parent Id"));
        }


        [TestMethod]
        public void Enrich_WithValidActivity_ProcessesCorrectly()
        {
            var factory = new Mock<ILogEventPropertyFactory>(MockBehavior.Strict);

            var messageTemplate = new MessageTemplate("test", new List<MessageTemplateToken>());
            var logEvent = new LogEvent(DateTime.UtcNow, LogEventLevel.Verbose, null, messageTemplate, new List<LogEventProperty>());
            var enricher = new OperationIdEnricher();

            var activity = new Activity("testOp");
            activity.Start();
            enricher.Enrich(logEvent, factory.Object);
            activity.Stop();

            Assert.IsTrue(logEvent.Properties.ContainsKey("Operation Id"));
            Assert.IsTrue(logEvent.Properties.ContainsKey("Parent Id"));
        }
    }
}
