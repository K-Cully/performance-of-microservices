
using CoreService.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoreService.Simulation.Processors
{
    /// <summary>
    /// Defines request processing configuration.
    /// </summary>
    [Serializable]
    public class Processor : IProcessor
    {
        /// <summary>
        /// Gets the error payload data.
        /// </summary>
        [JsonIgnore]
        public object ErrorPayload => new ErrorResponse(new string(new char[ErrorPayloadSize])); // TODO: clean up


        /// <summary>
        /// The size of the error payload.
        /// </summary>
        [JsonProperty("errorSize")]
        [JsonRequired]
        [Range(0, int.MaxValue, ErrorMessage = "errorSize cannot be negative")]
        public int ErrorPayloadSize { get; set; }


        /// <summary>
        /// The artificial latency to apply to incoming requests.
        /// </summary>
        [JsonProperty("latency")]
        [JsonRequired]
        [Range(0, int.MaxValue, ErrorMessage = "latency cannot be negative")]
        public int IngressLatencyMilliseconds { get; set; }


        /// <summary>
        /// The list of steps to perform as part of this processor.
        /// </summary>
        [JsonProperty("steps")]
        [JsonRequired]
        public IList<string> Steps { get; set; }


        /// <summary>
        /// The size of the success payload.
        /// </summary>
        [JsonProperty("successSize")]
        [JsonRequired]
        [Range(0, int.MaxValue, ErrorMessage = "successSize cannot be negative")]
        public int SuccessPayloadSize { get; set; }


        /// <summary>
        /// Gets the success payload data.
        /// </summary>
        [JsonIgnore]
        public object SuccessPayload => new SuccessResponse(new string(new char[SuccessPayloadSize])); // TODO: clean up
    }
}
