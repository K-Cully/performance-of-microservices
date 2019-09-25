using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CoreService.Simulation.Core
{
    /// <summary>
    /// Interface for loading and processing of emulated and simulated request components.
    /// </summary>
    public interface IEngine
    {
        /// <summary>
        /// Emulates the processing of the request based on the named configuration.
        /// </summary>
        /// <param name="name">The name of the processor configuration.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> instance indicating the overall processing result.
        /// </returns>
        Task<IActionResult> ProcessRequestAsync(string name);
    }
}