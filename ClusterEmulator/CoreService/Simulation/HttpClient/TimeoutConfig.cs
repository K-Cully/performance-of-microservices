using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly;

namespace CoreService.Simulation.HttpClient
{
    public class TimeoutConfig : IPolicyConfiguration
    {
        public IsPolicy AsPolicy()
        {
			// TODO
            throw new NotImplementedException();
        }
    }
}
