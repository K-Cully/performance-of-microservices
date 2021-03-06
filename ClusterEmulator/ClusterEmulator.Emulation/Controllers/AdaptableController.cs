﻿using ClusterEmulator.Emulation.Core;
using ClusterEmulator.Emulation.Logging;
using ClusterEmulator.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClusterEmulator.Emulation.Controllers
{
    /// <summary>
    /// An abstract controller that can be inherited to provide the configured API for a configurable service.
    /// </summary>
    [Produces("application/json")]
    public abstract class AdaptableController : ControllerBase
    {
        private readonly IEngine engine;
        private readonly ILogger<AdaptableController> log;
        private readonly IScopedLogContextFactory correlatedLog;


        /// <summary>
        /// Creates a new instance of <see cref="AdaptableController"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <param name="simulationEngine">The engine for performing simulated and emulated processing.</param>
        /// <param name="logContextFactory">The factory for creating correlated log contexts.</param>
        protected AdaptableController(ILogger<AdaptableController> logger, IEngine simulationEngine, IScopedLogContextFactory logContextFactory)
        {
            log = logger ?? throw new ArgumentNullException(nameof(logger));
            engine = simulationEngine ?? throw new ArgumentNullException(nameof(simulationEngine));
            correlatedLog = logContextFactory ?? throw new ArgumentNullException(nameof(logContextFactory));
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Preventing general exceptions being returned to the user")]
        private async Task<IActionResult> ProcessRequestAsync(string name, string caller)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new ErrorResponse($"{nameof(name)} is required"));
            }

            if (!string.IsNullOrWhiteSpace(caller))
            {
                log.LogDebug("{Caller} requested {Processor}", caller, name);
            }

            try
            {
                return await engine.ProcessRequestAsync(name).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                log.LogError(ex, "Request for {Processor} could not be processed", name);
                return BadRequest(new ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An unexpected error occurred in {Processor}", name);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }


        /// <summary>
        /// GET api/adaptable/name
        /// </summary>
        /// <param name="name">The name of the path to execute.</param>
        /// <param name="caller">The identity of the caller.</param>
        /// <returns>An action result indicating processing status.</returns>
        [HttpGet("{name}")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status418ImATeapot)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(string name, [FromQuery] string caller)
        {
            using (correlatedLog.InitializeFrom(ControllerContext.HttpContext))
            {
                return await ProcessRequestAsync(name, caller);
            }
        }


        /// <summary>
        /// DELETE api/adaptable/name
        /// </summary>
        /// <param name="name">The name of the path to execute.</param>
        /// <param name="caller">The identity of the caller.</param>
        /// <returns>An action result indicating processing status.</returns>
        [HttpDelete("{name}")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status418ImATeapot)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string name, [FromQuery] string caller)
        {
            using (correlatedLog.InitializeFrom(ControllerContext.HttpContext))
            {
                return await ProcessRequestAsync(name, caller);
            }
        }


        /// <summary>
        /// OPTIONS api/adaptable/name
        /// </summary>
        /// <param name="name">The name of the path to execute.</param>
        /// <param name="caller">The identity of the caller.</param>
        /// <returns>An action result indicating processing status.</returns>
        [HttpOptions("{name}")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status418ImATeapot)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Options(string name, [FromQuery] string caller)
        {
            using (correlatedLog.InitializeFrom(ControllerContext.HttpContext))
            {
                return await ProcessRequestAsync(name, caller);
            }
        }


        /// <summary>
        /// POST api/adaptable/name
        /// </summary>
        /// <param name="name">The name of the path to execute.</param>
        /// <param name="request">The request payload.</param>
        /// <param name="caller">The identity of the caller.</param>
        /// <returns>An action result indicating processing status.</returns>
        [HttpPost("{name}")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status418ImATeapot)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post(string name, [FromBody] AdaptableRequest request, [FromQuery] string caller)
        {
            using (correlatedLog.InitializeFrom(ControllerContext.HttpContext))
            {
                if (!ModelState.IsValid)
                {
                    IEnumerable<string> errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                    return BadRequest(new ErrorResponse($"['{string.Join("', '", errors)}']"));
                }

                if (request is null)
                {
                    log.LogError("A malformed request was received for {Processor}", name);
                    return BadRequest(new ErrorResponse($"{nameof(request)} is required"));
                }

                return await ProcessRequestAsync(name, caller);
            }
        }


        /// <summary>
        /// PUT api/adaptable/name
        /// </summary>
        /// <param name="name">The name of the path to execute.</param>
        /// <param name="request">The request payload.</param>
        /// <param name="caller">The identity of the caller.</param>
        /// <returns>An action result indicating processing status.</returns>
        [HttpPut("{name}")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status418ImATeapot)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(string name, [FromBody] AdaptableRequest request, [FromQuery] string caller)
        {
            using (correlatedLog.InitializeFrom(ControllerContext.HttpContext))
            {
                if (!ModelState.IsValid)
                {
                    IEnumerable<string> errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                    return BadRequest(new ErrorResponse($"['{string.Join("', '", errors)}']"));
                }

                if (request is null)
                {
                    log.LogError("A malformed request was received for {Processor}", name);
                    return BadRequest(new ErrorResponse($"{nameof(request)} is required"));
                }

                return await ProcessRequestAsync(name, caller);
            }
        }
    }
}