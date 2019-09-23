using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        private readonly INameProcessor m_nameProcessor;


        /// <summary>
        /// Creates a new instance of <see cref="NamesController"/>
        /// </summary>
        /// <param name="processor">The name processor</param>
        public NamesController(INameProcessor processor)
        {
            m_nameProcessor = processor;
        }


        // GET api/names
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> GetAsync()
        {
            return (await m_nameProcessor.GenerateNamesAsync(1)).ToList();
        }


        // GET api/names/5
        [HttpGet("{count}")]
        public async Task<ActionResult<IEnumerable<string>>> GetAsync(int count)
        {
            return (await m_nameProcessor.GenerateNamesAsync(count)).ToList();
        }
    }
}
