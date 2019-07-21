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
            if (!registry.TryGetProcessor(name, out IProcessor processor))
            {
                throw new InvalidOperationException($"Processor {name} is not registered");
            }

            if (processor == null)
            {
                throw new InvalidOperationException($"Processor {name} is null");
            }

            // TODO

            return name;
        }
    }
}
