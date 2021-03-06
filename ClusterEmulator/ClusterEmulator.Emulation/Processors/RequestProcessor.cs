﻿
using ClusterEmulator.Models;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace ClusterEmulator.Emulation.Processors
{
    /// <summary>
    /// Defines request processing configuration.
    /// </summary>
    [Serializable]
    public class RequestProcessor : Processor, IRequestProcessor
    {
        /// <summary>
        /// Gets the error payload data.
        /// </summary>
        [JsonIgnore]
        public ErrorResponse ErrorPayload => new ErrorResponse(new string('0', ErrorPayloadSize / 2)); 


        /// <summary>
        /// The size of the error payload in bytes.
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
        /// The size of the success payload in bytes.
        /// </summary>
        [JsonProperty("successSize")]
        [JsonRequired]
        [Range(0, int.MaxValue, ErrorMessage = "successSize cannot be negative")]
        public int SuccessPayloadSize { get; set; }


        /// <summary>
        /// Gets the success payload data.
        /// </summary>
        [JsonIgnore]
        public SuccessResponse SuccessPayload => new SuccessResponse(SuccessPayloadSize / 2);
    }
}
