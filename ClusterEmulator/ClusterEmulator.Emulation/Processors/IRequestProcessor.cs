using ClusterEmulator.Service.Models;

namespace ClusterEmulator.Emulation.Processors
{
    /// <summary>
    /// An interface to define request processing configuration.
    /// </summary>
    public interface IRequestProcessor : IProcessor
    {
        /// <summary>
        /// Gets the error payload data.
        /// </summary>
        ErrorResponse ErrorPayload { get; }


        /// <summary>
        /// The size of the error payload.
        /// </summary>
        int ErrorPayloadSize { get; set; }


        /// <summary>
        /// The artificial latency to apply to incoming requests.
        /// </summary>
        int IngressLatencyMilliseconds { get; set; }


        /// <summary>
        /// The size of the success payload.
        /// </summary>
        int SuccessPayloadSize { get; set; }


        /// <summary>
        /// Gets the success payload data.
        /// </summary>
        SuccessResponse SuccessPayload { get; }
    }
}