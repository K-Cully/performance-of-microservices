using System.Threading.Tasks;

namespace CoreService.Simulation.Steps
{
    /// <summary>
    /// Interface for all execution steps.
    /// </summary>
    public interface IStep
    {
        /// <summary>
        /// Executes the action defined by the step.
        /// </summary>
        /// <returns>A <see cref="ExecutionStatus"/> value.</returns>
        Task<ExecutionStatus> ExecuteAsync();
    }
}
