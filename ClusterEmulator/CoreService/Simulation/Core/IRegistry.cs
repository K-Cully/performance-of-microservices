using CoreService.Simulation.Processors;
using CoreService.Simulation.Steps;

namespace CoreService.Simulation.Core
{
    /// <summary>
    /// Interface for handling retrieval of simulation configuration for a service.
    /// </summary>
    public interface IRegistry
    {
        /// <summary>
        /// Retrieves the processor with a given name, if it is registered.
        /// </summary>
        /// <param name="name">The name of the processor.</param>
        /// <returns>The <see cref="Processor"/> instance.</returns>
        IProcessor GetProcessor(string name);


        /// <summary>
        /// Retrieves the step with a given name, if it is registered.
        /// </summary>
        /// <param name="name">The name of the step.</param>
        /// <returns>The <see cref="IStep"/> instance.</returns>
        IStep GetStep(string name);
    }
}