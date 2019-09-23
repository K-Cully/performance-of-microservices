using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RandomGeneratorService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RandomController : ControllerBase
    {
        // GET api/random
        [HttpGet]
        public async Task<ActionResult<int>> GetAsync()
        {
            // TODO: implement
            return await Task.FromResult(1);
        }
    }
}
