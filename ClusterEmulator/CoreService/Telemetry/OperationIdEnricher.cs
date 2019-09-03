using Serilog.Core;
using Serilog.Events;
using System;
using System.Diagnostics;

namespace CoreService.Telemetry
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

            Activity activity = Activity.Current;
            if (activity is null)
            {
                return;
            }

            logEvent.AddPropertyIfAbsent(new LogEventProperty("Operation Id", new ScalarValue(activity.RootId)));
            logEvent.AddPropertyIfAbsent(new LogEventProperty("Parent Id", new ScalarValue(activity.Id)));
        }
    }
}
