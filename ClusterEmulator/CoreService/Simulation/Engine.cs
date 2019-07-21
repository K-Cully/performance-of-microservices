using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreService.Simulation
{
    public class Engine
    {
        private readonly Registry registry;


        public Engine(Registry simulationRegistry)
        {
            registry = simulationRegistry ?? throw new ArgumentNullException(nameof(simulationRegistry));
        }


        public ActionResult<string> ProcessRequest(string name)
        {
            // TODO: explicit handling of excptions to avoid impaciting test results

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"{nameof(name)} cannot be null or whitespace", nameof(name));
            }

            IProcessor processor = registry.GetProcessor(name);

            // TODO: add success data
            ActionResult<string> result = new OkObjectResult(processor.SuccessPayload);


            foreach (string stepName in processor.Steps)
            {
                IStep step = registry.GetStep(stepName);
                ActionResult<string> currentResult = step.Execute();

                // TODO: if result not null or OkayResult, return
                if (currentResult is OkResult || currentResult is OkObjectResult)
                {

                }
            }

            // TODO

            return result;
        }
    }
}
