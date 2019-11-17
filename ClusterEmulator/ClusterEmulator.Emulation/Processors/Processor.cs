using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClusterEmulator.Emulation.Processors
{
    /// <summary>
    /// Defines basic processing configuration.
    /// </summary>
    public abstract class Processor : IProcessor
    {
        /// <summary>
        /// The list of steps to perform as part of this processor.
        /// </summary>
        [JsonProperty("steps")]
        [JsonRequired]
        public IList<string> Steps { get; set; }


        /// <summary>
        /// Conversion for factory finalization.
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <returns></returns>
        public IProcessor AsTypeModel(ILogger logger)
        {
            Log = logger;
            return this;
        }


        /// <summary>
        /// Gets the log instance.
        /// </summary>
        [JsonIgnore]
        protected ILogger Log { get; private set; }
    }
}
