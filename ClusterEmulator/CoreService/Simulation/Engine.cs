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
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"{nameof(name)} cannot be null or whitespace", nameof(name));
            }

            IProcessor processor = registry.GetProcessor(name);

            ActionResult<string> result = new ActionResult<string>(string.Empty);
            foreach (string stepName in processor.Steps)
            {
                IStep step = registry.GetStep(stepName);
                result = step.Execute();

                // TODO: if result not null or OkayResult, return
            }

            // TODO

            return result;
        }
    }
}
