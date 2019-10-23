using ClusterEmulator.Service.Simulation.Core;
using System.Collections.Generic;

namespace ClusterEmulator.Service.Simulation.Processors
{
    /// <summary>
    /// A generic interface for all processor types.
    /// </summary>
    public interface IProcessor : IConfigModel<IProcessor>
    {
        /// <summary>
        /// The list of steps to perform as part of this processor.
        /// </summary>
        IList<string> Steps { get; set; }
    }
}
