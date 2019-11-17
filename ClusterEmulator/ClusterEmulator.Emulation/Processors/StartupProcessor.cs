
using ClusterEmulator.Service.Models;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace ClusterEmulator.Emulation.Processors
{
    /// <summary>
    /// Defines request processing configuration.
    /// </summary>
    [Serializable]
    public class StartupProcessor : Processor, IStartupProcessor
    {
        /// <summary>
        /// A value indicating whether the processor should execute asynchronously or not.
        /// </summary>
        [JsonProperty("asynchronous")]
        [JsonRequired]
        public bool Asynchronous { get; set; }
    }
}
