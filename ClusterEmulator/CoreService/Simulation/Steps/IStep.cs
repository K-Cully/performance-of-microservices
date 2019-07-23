using Microsoft.AspNetCore.Mvc;

namespace CoreService.Simulation.Steps
{
    public interface IStep
    {
        // TODO: restrict
        string Name { get; set; }


        ActionResult<string> Execute();
    }
}
