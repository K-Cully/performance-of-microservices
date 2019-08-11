
using CoreService.Model;
using CoreService.Simulation.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class AdaptableController : ControllerBase
    {
        private readonly IEngine engine;


        /// <summary>
        /// Creates a new instance of <see cref="AdaptableController"/>.
        /// </summary>
        /// <param name="simulationEngine">The engine for performing simulated and emulated processing.</param>
        public AdaptableController(IEngine simulationEngine)
        {
            engine = simulationEngine ??
                throw new ArgumentNullException(nameof(simulationEngine), "Simulation engine must be initialized");
        }


        private async Task<IActionResult> ProcessRequestAsync(string name, string caller)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new ErrorResponse($"{nameof(name)} is required"));
            }

            if (!string.IsNullOrWhiteSpace(caller))
            {
                // TODO log caller, if present
            }

            try
            {
                return await engine.ProcessRequestAsync(name).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                // TODO: log exception
                return BadRequest(new ErrorResponse(ex.Message));
            }
            catch (Exception e)
            {
                // TODO: log exception
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
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(string name, [FromQuery] string caller)
        {
            return await ProcessRequestAsync(name, caller);
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
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string name, [FromQuery] string caller)
        {
            return await ProcessRequestAsync(name, caller);
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
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Options(string name, [FromQuery] string caller)
        {
            return await ProcessRequestAsync(name, caller);
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
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post(string name, [FromBody] AdaptableRequest request, [FromQuery] string caller)
        {
            if (!ModelState.IsValid)
            {
                IEnumerable<string> errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                return BadRequest(new ErrorResponse($"['{string.Join("', '", errors)}']"));
            }

            if (request is null)
            {
                return BadRequest(new ErrorResponse($"{nameof(request)} is required"));
            }

            return await ProcessRequestAsync(name, caller);
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
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(string name, [FromBody] AdaptableRequest request, [FromQuery] string caller)
        {
            if (!ModelState.IsValid)
            {
                IEnumerable<string> errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                return BadRequest(new ErrorResponse($"['{string.Join("', '", errors)}']"));
            }

            if (request is null)
            {
                return BadRequest(new ErrorResponse($"{nameof(request)} is required"));
            }

            return await ProcessRequestAsync(name, caller);
        }
    }
}