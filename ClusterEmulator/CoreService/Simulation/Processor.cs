
using System.Collections.Generic;

namespace CoreService.Simulation
{
    /// <summary>
    /// Defines request processing configuration.
    /// </summary>
    [System.Serializable]
    public class Processor : IProcessor
    {
        /// <summary>
        /// Gets the error payload data.
        /// </summary>
        public object ErrorPayload => new { error = new string(new char[ErrorPayloadSize]) }; // TODO: clean up


        /// <summary>
        /// The size of the error payload.
        /// </summary>
        public int ErrorPayloadSize { get; set; }


        /// <summary>
        /// The artificial latency to apply to incoming requests.
        /// </summary>
        public int IngressLatencyMilliseconds { get; set; }


        /// <summary>
        /// The list of steps to perform as part of this processor.
        /// </summary>
        public IList<string> Steps { get; set; }


        /// <summary>
        /// The size of the success payload.
        /// </summary>
        public int SuccessPayloadSize { get; set; }


        /// <summary>
        /// Gets the success payload data.
        /// </summary>
        public object SuccessPayload => new { result = new string(new char[SuccessPayloadSize]) }; // TODO: clean up
    }
}
