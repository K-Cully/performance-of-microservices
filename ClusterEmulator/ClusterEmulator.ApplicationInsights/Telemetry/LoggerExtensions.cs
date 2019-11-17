using Serilog;
using Serilog.Configuration;
using System;

namespace ClusterEmulator.ApplicationInsights.Telemetry
{
    /// <summary>
    /// Extensions for Serilog Logger components
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Convienence method to add an operation id to Application Insights logs.
        /// </summary>
        /// <param name="configuration">The log configuraiton.</param>
        /// <returns>The logger configuration.</returns>
        public static LoggerConfiguration WithOperationId(this LoggerEnrichmentConfiguration configuration)
        {
            _ = configuration ?? throw new ArgumentNullException(nameof(configuration));

            return configuration.With<OperationIdEnricher>();
        }
    }
}
