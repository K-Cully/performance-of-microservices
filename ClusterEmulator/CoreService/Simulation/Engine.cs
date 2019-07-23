using CoreService.Simulation.Steps;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreService.Simulation
{
    public class Engine : IEngine
    {
        private readonly IRegistry registry;


        public Engine(IRegistry simulationRegistry)
        {
            registry = simulationRegistry ?? throw new ArgumentNullException(nameof(simulationRegistry));
        }


        public async Task<IActionResult> ProcessRequest(string name)
        {
            // TODO: explicit handling of excptions to avoid impaciting test results

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"{nameof(name)} cannot be null or whitespace", nameof(name));
            }

            IProcessor processor = registry.GetProcessor(name);
            foreach (string stepName in processor.Steps)
            {
                IStep step = registry.GetStep(stepName);

                // TODO: await
                ExecutionStatus status = await step.ExecuteAsync();

                // TODO: if result not null or OkayResult, return
                switch (status)
                {
                    case ExecutionStatus.Fail:
                        ObjectResult result = new ObjectResult(processor.ErrorPayload)
                        { StatusCode = StatusCodes.Status500InternalServerError };
                        return result;
                    case ExecutionStatus.Success:
                    case ExecutionStatus.Unexpected:
                    default:
                        // TODO: handle specific cases
                        break;
                }
            }

            return new OkObjectResult(processor.SuccessPayload);
        }
    }
}