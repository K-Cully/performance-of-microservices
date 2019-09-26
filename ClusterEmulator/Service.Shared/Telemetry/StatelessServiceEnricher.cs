using Serilog.Core;
using Serilog.Events;
using System;
using System.Fabric;

namespace ClusterEmulator.Service.Shared.Telemetry
{
    /// <summary>
    /// Enriches a log event with stateless service context information.
    /// </summary>
    public class StatelessServiceEnricher : ILogEventEnricher
    {
        private StatelessServiceContext Context { get; }

        private LogEventProperty serviceTypeName;

        private LogEventProperty serviceName;

        private LogEventProperty partitionId;

        private LogEventProperty instanceId;

        private LogEventProperty nodeName;


        /// <summary>
        /// Creates an instance of <see cref="StatelessServiceEnricher"/>
        /// </summary>
        /// <param name="context">The service context to retrieve information from.</param>
        public StatelessServiceEnricher(StatelessServiceContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }


        /// <summary>
        /// Enriches a log event with service context information.
        /// </summary>
        /// <param name="logEvent">The current log event.</param>
        /// <param name="propertyFactory">A factory for creating log envent properties.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            _ = logEvent ?? throw new ArgumentNullException(nameof(logEvent));
            _ = propertyFactory ?? throw new ArgumentNullException(nameof(propertyFactory));

            serviceTypeName = serviceTypeName ?? propertyFactory.CreateProperty("ServiceTypeName", Context.ServiceTypeName);
            serviceName = serviceName ?? propertyFactory.CreateProperty("ServiceName", Context.ServiceName);
            partitionId = partitionId ?? propertyFactory.CreateProperty("PartitionId", Context.PartitionId);
            instanceId = instanceId ?? propertyFactory.CreateProperty("InstanceId", Context.InstanceId);
            nodeName = nodeName ?? propertyFactory.CreateProperty("NodeName", Context.NodeContext?.NodeName);

            logEvent.AddPropertyIfAbsent(serviceTypeName);
            logEvent.AddPropertyIfAbsent(serviceName);
            logEvent.AddPropertyIfAbsent(partitionId);
            logEvent.AddPropertyIfAbsent(instanceId);
            logEvent.AddPropertyIfAbsent(nodeName);
        }
    }
}
