using Microsoft.AspNetCore.Mvc;

namespace CoreService.Simulation
{
    public interface IEngine
    {
        ActionResult<string> ProcessRequest(string name);
    }
}