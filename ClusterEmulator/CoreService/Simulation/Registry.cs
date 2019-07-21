using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreService.Simulation
{
    public class Registry
    {
        public IDictionary<string, Processor> Processors { get; set; }


        public IDictionary<string, Step> Steps { get; set; } // TODO restrict access
    }
}
