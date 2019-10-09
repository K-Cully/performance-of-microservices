using ClusterEmulator.Service.Shared.Telemetry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClusterEmulator.Service.Shared.Test.Telemetry
{
    [TestClass]
    public class AppInsightsTelemetryConverterUnitTests
    {
        [TestMethod]
        public void Convert_WithNullEvent_ThrowsException()
        {
            var formatProvider = new Mock<IFormatProvider>(MockBehavior.Strict);
            var converter = new AppInsightsTelemetryConverter();

            var deferredConversion = converter.Convert(null, formatProvider.Object);

            Assert.IsNotNull(deferredConversion);
            Assert.ThrowsException<ArgumentNullException>(
                () => deferredConversion.ToList());
        }


        [TestMethod]
        public void Convert_WithoutOpertionIds_ExecutesCorrectly()
        {
            var messageTemplate = new MessageTemplate("test", new List<MessageTemplateToken>());
            var logEvent = new LogEvent(DateTime.UtcNow, LogEventLevel.Verbose, null, messageTemplate, new List<LogEventProperty>());
            var converter = new AppInsightsTelemetryConverter();

            var deferredConversion = converter.Convert(logEvent, null);

            Assert.IsNotNull(deferredConversion);
            var telemetry = deferredConversion.ToList();

            Assert.IsNotNull(telemetry);
            Assert.IsTrue(telemetry.Any());
            Assert.IsFalse(telemetry.Any(t => t?.Context?.Operation?.Id != null));
            Assert.IsFalse(telemetry.Any(t => t?.Context?.Operation?.ParentId != null));
        }


        [TestMethod]
        public void Convert_WithOpertionId_ExecutesCorrectly()
        {
            var formatProvider = new Mock<IFormatProvider>(MockBehavior.Strict);
            var messageTemplate = new MessageTemplate("test", new List<MessageTemplateToken>());
            var properties = new List<LogEventProperty>()
            {
                new LogEventProperty(PropertyNames.OperationId, new ScalarValue("testOperation"))
            };
            var logEvent = new LogEvent(DateTime.UtcNow, LogEventLevel.Verbose, null, messageTemplate, properties);
            var converter = new AppInsightsTelemetryConverter();

            var deferredConversion = converter.Convert(logEvent, formatProvider.Object);

            Assert.IsNotNull(deferredConversion);
            var telemetry = deferredConversion.ToList();

            Assert.IsNotNull(telemetry);
            Assert.IsTrue(telemetry.Any(t => t?.Context?.Operation?.Id != null));
            Assert.IsFalse(telemetry.Any(t => t?.Context?.Operation?.ParentId != null));
        }
    }
}
