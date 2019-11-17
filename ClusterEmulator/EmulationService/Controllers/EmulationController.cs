using ClusterEmulator.Service.Shared;
using ClusterEmulator.Service.Shared.Telemetry;
using ClusterEmulator.Emulation.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmulationService.Controllers
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
        /// <param name="logContextFactory">The factory for creating correlated log contexts.</param>
        public EmulationController(ILogger<EmulationController> logger, IEngine simulationEngine, IScopedLogContextFactory logContextFactory)
            : base(logger, simulationEngine, logContextFactory)
        {}
    }
}