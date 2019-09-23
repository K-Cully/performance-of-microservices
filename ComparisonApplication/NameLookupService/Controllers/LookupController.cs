using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NameLookupService.Core;

namespace NameLookupService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        private readonly INameStore m_store;


        /// <summary>
        /// Creates a new <see cref="LookupController"/>
        /// </summary>
        /// <param name="store">The name store to query</param>
        public LookupController(INameStore store)
        {
            m_store = store;
        }


        // GET api/lookup/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<string>>> GetAsync(int id)
        {
            string result = await m_store.GetNameAsync(id).ConfigureAwait(false);
            if (string.IsNullOrEmpty(result))
            {
                return NoContent();
            }

            return new List<string> { result };
        }


        // POST api/lookup
        [HttpPost]
        public async Task<ActionResult<IEnumerable<string>>> PostAsync([FromBody] IEnumerable<int> ids)
        {
            var tasks = new List<Task<string>>();
            foreach (int id in ids)
            {
                tasks.Add(m_store.GetNameAsync(id));
            }

            // TODO: deal with missing entries
            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
