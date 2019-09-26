using ClusterEmulator.Service.Shared;
using ClusterEmulator.Service.Simulation.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RandomGeneratorService.Controllers
{
    /// <summary>
    /// Provides a service specific controller with the required configuration.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RandomController : AdaptableController
    {
        /// <summary>
        /// Creates a new instance of <see cref="RandomController"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <param name="simulationEngine">The engine for performing simulated and emulated processing.</param>
        public RandomController(ILogger<RandomController> logger, IEngine simulationEngine)
            : base(logger, simulationEngine)
        {}
    }
}