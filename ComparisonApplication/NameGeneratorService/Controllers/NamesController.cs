using System.Collections.Generic;
using System.Linq;
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
        public ActionResult<string> Get()
        {
            return m_nameProcessor.GenerateNames(1).FirstOrDefault();
        }


        // GET api/names/5
        [HttpGet("{count}")]
        public ActionResult<IEnumerable<string>> Get(int count)
        {
            return m_nameProcessor.GenerateNames(count).ToList();
        }
    }
}
