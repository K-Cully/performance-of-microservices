using Microsoft.ApplicationInsights.Channel;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;
using System;
using System.Collections.Generic;

namespace ClusterEmulator.Service.Shared.Telemetry
{
    /// <summary>
    /// Converts trace logs containing operation ids to Application Insights telemetry with correct operation id tracking
    /// </summary>
    public class AppInsightsTelemetryConverter : TraceTelemetryConverter
    {
        /// <summary>
        /// Converts the oeration id dat from the log event into telemetry usable by Application Insights
        /// </summary>
        /// <param name="logEvent">The log event to process</param>
        /// <param name="formatProvider">The format provider</param>
        /// <returns>The converted telemetry entries</returns>
        public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            _ = logEvent ?? throw new ArgumentNullException(nameof(logEvent));

            foreach (var telemetry in base.Convert(logEvent, formatProvider))
            {
                if (TryGetScalarProperty(logEvent, PropertyNames.OperationId, out var operationId))
                    telemetry.Context.Operation.Id = operationId.ToString();

                if (TryGetScalarProperty(logEvent, PropertyNames.ParentId, out var parentId))
                    telemetry.Context.Operation.ParentId = parentId.ToString();

                yield return telemetry;
            }
        }


        private bool TryGetScalarProperty(LogEvent logEvent, string propertyName, out object value)
        {
            var hasScalarValue =
                logEvent.Properties.TryGetValue(propertyName, out var someValue) &&
                (someValue is ScalarValue);

            value = hasScalarValue ? ((ScalarValue)someValue).Value : default;
            return hasScalarValue;
        }
    }
}
