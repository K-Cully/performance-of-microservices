using ClusterEmulator.Emulation.Controllers;
using ClusterEmulator.Emulation.Core;
using ClusterEmulator.Emulation.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TemplateService.Controllers
{
    /// <summary>
    /// Provides a service specific controller with the required configuration.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TemplateController : AdaptableController
    {
        /// <summary>
        /// Creates a new instance of <see cref="TemplateController"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <param name="simulationEngine">The engine for performing simulated and emulated processing.</param>
        /// <param name="logContextFactory">The factory for creating correlated log contexts.</param>
        public TemplateController(ILogger<TemplateController> logger, IEngine simulationEngine, IScopedLogContextFactory logContextFactory)
            : base(logger, simulationEngine, logContextFactory)
        {}
    }
}