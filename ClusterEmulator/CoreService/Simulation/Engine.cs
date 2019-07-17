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
            // TODO
            return name;
        }
    }
}
