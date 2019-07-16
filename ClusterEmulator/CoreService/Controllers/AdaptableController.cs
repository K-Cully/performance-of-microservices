
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Controllers
{
    // TODO: return type JSON
    [Route("api/[controller]")]
    [ApiController]
    public class AdaptableController : ControllerBase
    {
        private ActionResult ProcessRequest(string name, string caller)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest();
            }

            if (!string.IsNullOrWhiteSpace(caller))
            {
                // TODO log caller, if present
            }

            // TODO: serialize some standard result object to reflect a true system
            return Ok();
        }


        /// <summary>
        /// GET api/adaptable/name
        /// </summary>
        /// <param name="name">The name of the path to execute.</param>
        /// <param name="caller">The identity of the caller.</param>
        /// <returns>A list of strings representing the simulated response payload.</returns>
        [HttpGet("{name}")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<IEnumerable<string>> Get(string name, [FromQuery] string caller)
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
            return Ok(new string[] { "test" });
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
        public ActionResult Delete(string name, [FromQuery] string caller)
        {
            return ProcessRequest(name, caller);
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
        public ActionResult Options(string name, [FromQuery] string caller)
        {
            return ProcessRequest(name, caller);
        }


        /// <summary>
        /// POST api/adaptable/name
        /// </summary>
        /// <param name="name">The name of the path to execute.</param>
        /// <param name="payload">The request payload.</param>
        /// <param name="caller">The identity of the caller.</param>
        /// <returns>An action result indicating processing status.</returns>
        [HttpPost("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Post(string name, [FromBody] string payload, [FromQuery] string caller)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return BadRequest($"{nameof(payload)} is required");
            }

            // TODO: do I need to process payload? (should probably desearilize to some custom type to reflect load)

            return ProcessRequest(name, caller);
        }


        /// <summary>
        /// PUT api/adaptable/name
        /// </summary>
        /// <param name="name">The name of the path to execute.</param>
        /// <param name="payload">The request payload.</param>
        /// <param name="caller">The identity of the caller.</param>
        /// <returns>An action result indicating processing status.</returns>
        [HttpPut("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Put(string name, [FromBody] string payload, [FromQuery] string caller)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return BadRequest($"{nameof(payload)} is required");
            }

            // TODO: do I need to process payload? (should probably desearilize to some custom type to reflect load)

            return ProcessRequest(name, caller);
        }
    }
}