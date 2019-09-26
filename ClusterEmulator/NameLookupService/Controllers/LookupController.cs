using ClusterEmulator.Service.Shared;
using ClusterEmulator.Service.Simulation.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace NameLookupService.Controllers
{
    /// <summary>
    /// Provides a service specific controller with the required configuration.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : AdaptableController
    {
        /// <summary>
        /// Creates a new instance of <see cref="CoreController"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <param name="simulationEngine">The engine for performing simulated and emulated processing.</param>
        public LookupController(ILogger<LookupController> logger, IEngine simulationEngine)
            : base(logger, simulationEngine)
        {}
    }
}