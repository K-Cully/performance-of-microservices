using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RandomGeneratorService.Core;

namespace RandomGeneratorService.Controllers
{
    /// <summary>
    /// Controller for dealing with requests for random numbers
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RandomController : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RandomController"/>
        /// </summary>
        /// <param name="processor">The processor to deal with requests</param>
        /// <param name="logger">The application trace logger</param>
        public RandomController(IRandomProcessor processor, ILogger<RandomController> logger)
        {
            Processor = processor ?? throw new ArgumentNullException(nameof(processor));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private IRandomProcessor Processor { get; }

        private ILogger Logger { get; }


        // GET api/random
        [HttpGet("{max}")]
        public async Task<ActionResult<int>> GetAsync(int max)
        {
            int number = await Processor.GetRandomNumber(max).ConfigureAwait(false);
            if (number < 0)
            {
                Logger.LogError("{Action} returned an invalid value: {Value}",
                    nameof(Processor.GetRandomNumber), number);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return number; 
        }
    }
}
