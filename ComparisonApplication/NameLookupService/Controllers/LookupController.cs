using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NameLookupService.Core;

namespace NameLookupService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        private INameStore Store { get; }

        private ILogger Logger { get; }


        /// <summary>
        /// Creates a new <see cref="LookupController"/>
        /// </summary>
        /// <param name="store">The name store to query</param>
        /// <param name="logger">The application trace logger</param>
        public LookupController(INameStore store, ILogger<LookupController> logger)
        {
            Store = store ?? throw new ArgumentNullException(nameof(store));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        // GET api/lookup/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<string>>> GetAsync(int id)
        {
            string result = await Store.GetNameAsync(id).ConfigureAwait(false);
            if (string.IsNullOrEmpty(result))
            {
                Logger.LogWarning("ID:{NameId} was not found in the name store", id);
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
                tasks.Add(Store.GetNameAsync(id));
            }

            List<string> names = (await Task.WhenAll(tasks).ConfigureAwait(false)).ToList();
            IEnumerable<string> blankNames = names.Where(n => string.IsNullOrWhiteSpace(n));
            if (blankNames.Any())
            {
                Logger.LogWarning("Some IDs were not found in the name store. {NameIdList}", ids);
            }

            return names;
        }
    }
}
