
using System.Collections.Generic;

namespace CoreService.Simulation
{
    public class Processor : IProcessor
    {
        // TODO: restrict?

        public string Name { get; set; }


        public int IngressLatencyMilliseconds { get; set; }


        public int ErrorPayloadSize { get; set; }


        public int SuccessPayloadSize { get; set; }


        public IList<string> Steps { get; set; }


        public string ErrorPayload => new string(new char[ErrorPayloadSize]); // TODO: clean up


        public string SuccessPayload => new string(new char[SuccessPayloadSize]); // TODO: clean up
    }
}
