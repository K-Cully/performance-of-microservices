using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NameGeneratorService.Core;

namespace NameGeneratorService.Controllers
{
    /// <summary>
    /// Controller for handling requests to generate a number of random names
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class NamesController : ControllerBase
    {
        private INameProcessor NameProcessor { get; }

        private ILogger Logger { get; }


        /// <summary>
        /// Creates a new instance of <see cref="NamesController"/>
        /// </summary>
        /// <param name="processor">The name processor</param>
        /// <param name="logger">The application trace logger</param>
        public NamesController(INameProcessor processor, ILogger<NamesController> logger)
        {
            NameProcessor = processor ?? throw new ArgumentNullException(nameof(processor));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        // GET api/names
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> GetAsync()
        {
            string name = await NameProcessor.GenerateNameAsync().ConfigureAwait(false);
            if (name is null)
            {
                Logger.LogError("{Operation} failed. An unexpected error occurred",
                    nameof(NameProcessor.GenerateNameAsync));
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                Logger.LogError("{Operation} failed to generate a valid name",
                    nameof(NameProcessor.GenerateNameAsync));
            }

            return new List<string> { name };
        }


        // GET api/names/5
        [HttpGet("{count}")]
        public async Task<ActionResult<IEnumerable<string>>> GetAsync(int count)
        {
            IEnumerable<string> names = (await NameProcessor.GenerateNamesAsync(count).ConfigureAwait(false));
            if (names is null)
            {
                Logger.LogError("{Operation} failed. An unexpected error occurred",
                    nameof(NameProcessor.GenerateNamesAsync));
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            var nameList = names.ToList();
            if (nameList.Any(name => string.IsNullOrWhiteSpace(name)))
            {
                Logger.LogError("{Operation} failed to generate all {NameCount} valid names",
                    nameof(NameProcessor.GenerateNameAsync), count);
            }

            return nameList;
        }
    }
}
