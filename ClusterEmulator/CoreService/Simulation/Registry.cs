using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreService.Simulation
{
    public class Registry
    {
        public Registry()
        {
            // TODO: load settings
        }


        public bool TryGetProcessor(string name, out IProcessor processor)
        {
            return Processors.TryGetValue(name, out processor);
        }


        private IDictionary<string, IProcessor> Processors { get; set; }


        private IDictionary<string, IStep> Steps { get; set; }
    }
}
