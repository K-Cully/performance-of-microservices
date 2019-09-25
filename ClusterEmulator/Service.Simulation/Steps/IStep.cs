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
