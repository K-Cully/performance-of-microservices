using ClusterEmulator.Service.Shared;
using ClusterEmulator.Service.Simulation.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace NameGeneratorService.Controllers
{
    /// <summary>
    /// Provides a service specific controller with the required configuration.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class NamesController : AdaptableController
    {
        /// <summary>
        /// Creates a new instance of <see cref="NamesController"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <param name="simulationEngine">The engine for performing simulated and emulated processing.</param>
        public NamesController(ILogger<NamesController> logger, IEngine simulationEngine)
            : base(logger, simulationEngine)
        {}
    }
}