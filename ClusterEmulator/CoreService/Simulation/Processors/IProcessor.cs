using System.Collections.Generic;

namespace CoreService.Simulation.Processors
{
    /// <summary>
    /// An interface to define request processing configuration.
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// Gets the error payload data.
        /// </summary>
        object ErrorPayload { get; }


        /// <summary>
        /// The size of the error payload.
        /// </summary>
        int ErrorPayloadSize { get; set; }


        /// <summary>
        /// The artificial latency to apply to incoming requests.
        /// </summary>
        int IngressLatencyMilliseconds { get; set; }


        /// <summary>
        /// The list of steps to perform as part of this processor.
        /// </summary>
        IList<string> Steps { get; set; }


        /// <summary>
        /// The size of the success payload.
        /// </summary>
        int SuccessPayloadSize { get; set; }


        /// <summary>
        /// Gets the success payload data.
        /// </summary>
        object SuccessPayload { get; }
    }
}