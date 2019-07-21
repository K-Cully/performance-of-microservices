
using System.Collections.Generic;

namespace CoreService.Simulation
{
    public class Processor
    {
        // TODO: restrict?

        public string Name { get; set; }


        public int IngressLatencyMilliseconds { get; set; }


        public int ErrorPayloadSize { get; set; }


        public int SuccessPayloadSize { get; set; }


        public IList<string> Steps { get; set; }
    }
}
