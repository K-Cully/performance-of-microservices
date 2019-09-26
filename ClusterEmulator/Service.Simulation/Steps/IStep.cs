using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ClusterEmulator.Service.Simulation.Steps
{
    /// <summary>
    /// Interface for all execution steps.
    /// </summary>
    public interface IStep
    {
        /// <summary>
        /// The number of times the step should be executed in parallel.
        /// </summary>
        uint? ParallelCount { get; set; }


        /// <summary>
        /// The clause used to decide what parallel executions should fail the entire step.
        /// </summary>
        GroupClause FailOnParallelFailures { get; set; }


        /// <summary>
        /// Executes the action defined by the step.
        /// </summary>
        /// <returns>A <see cref="ExecutionStatus"/> value.</returns>
        Task<ExecutionStatus> ExecuteAsync();


        /// <summary>
        /// Initializes a logger for the step instance.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        void InitializeLogger(ILogger logger);
    }
}
