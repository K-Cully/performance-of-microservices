using System.Collections.Generic;

namespace CoreService.Simulation
{
    public interface IProcessor
    {
        int ErrorPayloadSize { get; set; }
        int IngressLatencyMilliseconds { get; set; }
        string Name { get; set; }
        IList<string> Steps { get; set; }
        int SuccessPayloadSize { get; set; }
    }
}