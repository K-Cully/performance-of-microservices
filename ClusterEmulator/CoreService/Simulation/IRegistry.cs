using CoreService.Simulation.Steps;
using System.Fabric.Description;

namespace CoreService.Simulation
{
    public interface IRegistry
    {
        IProcessor GetProcessor(string name);


        IStep GetStep(string name);
    }
}