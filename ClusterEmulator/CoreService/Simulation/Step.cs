using Microsoft.AspNetCore.Mvc;

namespace CoreService.Simulation
{
    public abstract class Step
    {
        public string Name { get; set; }


        public abstract ActionResult<string> Execute();
    }
}
