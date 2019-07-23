using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Simulation.Steps
{
    public class LoadStep : IStep
    {
        public string Name
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public async Task<ExecutionStatus> Execute()
        {
            // TODO
            return await Task.Run(() => ExecutionStatus.Success);
        }
    }
}
