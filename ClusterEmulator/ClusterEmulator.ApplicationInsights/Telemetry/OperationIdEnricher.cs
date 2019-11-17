using Serilog.Core;
using Serilog.Events;
using System;

namespace ClusterEmulator.ApplicationInsights.Telemetry
{
    /// <summary>
    /// Enriches log data with an operation and parent operation id
    /// </summary>
    public class OperationIdEnricher : ILogEventEnricher
    {
        /// <summary>
        /// Enriches the log event 
        /// </summary>
        /// <param name="logEvent">The log event to enrich</param>
        /// <param name="propertyFactory">Redundant property factory</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            _ = logEvent ?? throw new ArgumentNullException(nameof(logEvent));

            if (logEvent.Properties.TryGetValue(PropertyNames.RequestId, out var requestId))
            {
                logEvent.AddPropertyIfAbsent(new LogEventProperty(PropertyNames.OperationId, new ScalarValue(requestId)));
            }
        }
    }
}
