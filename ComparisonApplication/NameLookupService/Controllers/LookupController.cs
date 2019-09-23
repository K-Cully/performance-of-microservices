using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NameLookupService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        // GET api/lookup/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<string>>> GetAsync(int id)
        {
            // TODO: implement
            return new List<string>();
        }


        // POST api/lookup
        [HttpPost]
        public async Task<ActionResult<IEnumerable<string>>> PostAsync([FromBody] IEnumerable<int> ids)
        {
            // TODO: implement
            return new List<string>();
        }
    }
}
