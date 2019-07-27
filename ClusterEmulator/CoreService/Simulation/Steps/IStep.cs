using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CoreService.Simulation.Steps
{
    public interface IStep
    {
        Task<ExecutionStatus> ExecuteAsync();
    }
}
