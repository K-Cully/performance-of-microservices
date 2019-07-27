using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Simulation.Steps
{
    public class LoadStep : IStep
    {
        // TODO

        public string Name
        {
            get;
            set;
        }

        public async Task<ExecutionStatus> ExecuteAsync()
        {
            // TODO
            return await Task.Run(() => ExecutionStatus.Success);
        }
    }
}
