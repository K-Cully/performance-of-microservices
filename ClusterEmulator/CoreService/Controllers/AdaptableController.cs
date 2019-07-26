
using CoreService.Model;
using CoreService.Simulation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CoreService.Controllers
{
    // TODO: make calls use async - await
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
                throw new ArgumentException("Simulation engine must be initialized", nameof(simulationEngine));
        }


        private async Task<IActionResult> ProcessRequestAsync(string name, string caller)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest();
            }

            if (!string.IsNullOrWhiteSpace(caller))
            {
                // TODO log caller, if present
            }

            try
            {
                IActionResult result = await engine.ProcessRequest(name).ConfigureAwait(false);
                return result;
            }
            catch(Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                // TODO: log and add error details
                return BadRequest();
            }
            catch (Exception ex)
            {
                // TODO: log and add error details
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }


        /// <summary>
        /// GET api/adaptable/name
        /// </summary>
        /// <param name="name">The name of the path to execute.</param>
        /// <param name="caller">The identity of the caller.</param>
        /// <returns>A list of strings representing the simulated response payload.</returns>
        [HttpGet("{name}")]
        [ProducesResponseType(typeof(AdaptableResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get(string name, [FromQuery] string caller)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest();
            }

            if (!string.IsNullOrWhiteSpace(caller))
            {
                // TODO log caller, if present
            }

            // TODO: return set of standard values, based on payload size
            //return Ok(new AdaptableResponse(){ Values = new string[] { "test" } });

            return await ProcessRequestAsync(name, caller);
        }


        /// <summary>
        /// DELETE api/adaptable/name
        /// </summary>
        /// <param name="name">The name of the path to execute.</param>
        /// <param name="caller">The identity of the caller.</param>
        /// <returns>An action result indicating processing status.</returns>
        [HttpDelete("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(string name, [FromBody] AdaptableRequest request, [FromQuery] string caller)
        {
            if (request == null)
            {
                return BadRequest($"{nameof(request)} is required");
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put(string name, [FromBody] AdaptableRequest request, [FromQuery] string caller)
        {
            if (request == null)
            {
                return BadRequest($"{nameof(request)} is required");
            }

            return await ProcessRequestAsync(name, caller);
        }
    }
}