using System.Threading.Tasks;
using ClusterEmulator.Service.Simulation.Core;

namespace ClusterEmulator.Service.Simulation.Steps
{
    /// <summary>
    /// Interface for all execution steps.
    /// </summary>
    public interface IStep : IConfigModel<IStep>
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
    }
}
