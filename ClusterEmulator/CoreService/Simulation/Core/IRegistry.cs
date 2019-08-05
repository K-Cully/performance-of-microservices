using CoreService.Simulation.Processors;
using CoreService.Simulation.Steps;
using Polly;
using Polly.Registry;

namespace CoreService.Simulation.Core
{
    /// <summary>
    /// Interface for handling retrieval of simulation configuration for a service.
    /// </summary>
    public interface IRegistry
    {
        /// <summary>
        /// Gets the policy registry
        /// </summary>
        IPolicyRegistry<string> PolicyRegistry { get; }


        /// <summary>
        /// Retrieves the policy with a given name, if it is registered.
        /// </summary>
        /// <param name="name">The name of the policy.</param>
        /// <returns>The <see cref="IsPolicy"/> instance.</returns>
        IsPolicy GetPolicy(string name);


        /// <summary>
        /// Retrieves the processor with a given name, if it is registered.
        /// </summary>
        /// <param name="name">The name of the processor.</param>
        /// <returns>The <see cref="IProcessor"/> instance.</returns>
        IProcessor GetProcessor(string name);


        /// <summary>
        /// Retrieves the step with a given name, if it is registered.
        /// </summary>
        /// <param name="name">The name of the step.</param>
        /// <returns>The <see cref="IStep"/> instance.</returns>
        IStep GetStep(string name);
    }
}