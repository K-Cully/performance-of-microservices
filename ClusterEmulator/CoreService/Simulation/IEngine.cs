using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CoreService.Simulation
{
    public interface IEngine
    {
        Task<IActionResult> ProcessRequest(string name);
    }
}