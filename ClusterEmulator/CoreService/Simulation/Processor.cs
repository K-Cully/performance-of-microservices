
using System.Collections.Generic;

namespace CoreService.Simulation
{
    public class Processor
    {
        // TODO: restrict?

        public int IngressLatencyMilliseconds { get; set; }


        public int ErrorPayloadSize { get; set; }


        public int SuccessPayloadSize { get; set; }


        public IEnumerable<string> Steps { get; set; }
    }
}
