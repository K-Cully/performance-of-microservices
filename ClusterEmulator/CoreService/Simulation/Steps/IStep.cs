using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CoreService.Simulation.Steps
{
    public interface IStep
    {
        // TODO: restrict
        string Name { get; set; }


        Task<ExecutionStatus> ExecuteAsync();
    }
}
