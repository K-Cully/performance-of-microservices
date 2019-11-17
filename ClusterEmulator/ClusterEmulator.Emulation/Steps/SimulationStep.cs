using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ClusterEmulator.Emulation.Steps
{
    /// <summary>
    /// Abstract base class for all execution steps.
    /// </summary>
    public abstract class SimulationStep : IStep
    {
        [JsonIgnore]
        private ILogger log;


        /// <summary>
        /// A logger for use throughout the step's execution.
        /// </summary>
        [JsonIgnore]
        protected ILogger Logger { get => log; set => log = log ?? value; }


        /// <summary>
        /// The number of times the step should be executed in parallel.
        /// </summary>
        [JsonProperty("parallelCount")]
        public uint? ParallelCount { get; set; }


        /// <summary>
        /// The clause used to decide what parallel executions should fail the entire step.
        /// </summary>
        [JsonProperty("parallelError")]
        public GroupClause FailOnParallelFailures { get; set; }


        /// <summary>
        /// Initializes a logger for the step instance.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        public IStep AsTypeModel(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            return this;
        }


        /// <summary>
        /// Executes the action defined by the step.
        /// </summary>
        /// <returns>A <see cref="ExecutionStatus"/> value.</returns>
        public abstract Task<ExecutionStatus> ExecuteAsync();
    }
}
