using ClusterEmulator.Service.Simulation.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ClusterEmulator.Service.Shared
{
    /// <summary>
    /// Provides a service specific controller with the required configuration.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EmulationController : AdaptableController
    {
        /// <summary>
        /// Creates a new instance of <see cref="EmulationController"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <param name="simulationEngine">The engine for performing simulated and emulated processing.</param>
        public EmulationController(ILogger<EmulationController> logger, IEngine simulationEngine)
            : base(logger, simulationEngine)
        {}
    }
}