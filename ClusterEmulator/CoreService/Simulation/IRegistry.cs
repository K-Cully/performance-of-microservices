using CoreService.Simulation.Steps;

namespace CoreService.Simulation
{
    public interface IRegistry
    {
        IProcessor GetProcessor(string name);
        IStep GetStep(string name);
    }
}