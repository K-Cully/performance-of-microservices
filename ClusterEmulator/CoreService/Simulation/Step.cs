using Microsoft.AspNetCore.Mvc;

namespace CoreService.Simulation
{
    public interface IStep
    {
        // TODO: restrict
        string Name { get; set; }


        ActionResult<string> Execute();
    }
}
