using Serilog.Core;
using Serilog.Events;
using System;
using System.Diagnostics;

namespace ClusterEmulator.Service.Shared.Telemetry
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


            // TODO: allow use of HttpContext.Current.RequestId or other values

            Activity activity = Activity.Current;
            if (activity is null)
            {
                return;
            }

            logEvent.AddPropertyIfAbsent(new LogEventProperty(PropertyNames.OperationId, new ScalarValue(activity.RootId)));
            logEvent.AddPropertyIfAbsent(new LogEventProperty(PropertyNames.ParentId, new ScalarValue(activity.Id)));
        }
    }
}
