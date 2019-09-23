using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        private IRandomProcessor m_processor;


        /// <summary>
        /// Initializes a new instance of <see cref="RandomController"/>
        /// </summary>
        /// <param name="processor">The processor to deal with requests</param>
        public RandomController(IRandomProcessor processor)
        {
            m_processor = processor ?? throw new ArgumentNullException(nameof(processor));
        }


        // GET api/random
        [HttpGet("{max}")]
        public async Task<ActionResult<int>> GetAsync(int max)
        {
            // TODO: restrict range of max

            return await m_processor.GetRandomNumber(max).ConfigureAwait(false);
        }
    }
}
