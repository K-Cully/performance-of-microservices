using Microsoft.AspNetCore.Mvc;

namespace CoreService.Simulation
{
    public interface IEngine
    {
        IActionResult ProcessRequest(string name);
    }
}