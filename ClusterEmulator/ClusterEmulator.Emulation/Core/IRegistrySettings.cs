using System.Collections.Generic;

namespace ClusterEmulator.Emulation.Core
{
    /// <summary>
    /// Interface to facilitate flexible registry setting loading
    /// </summary>
    public interface IRegistrySettings
    {
        /// <summary>
        /// Attempts to retrieve the enumeration of settings from a configuration section
        /// </summary>
        /// <param name="name">The name of the section to retrieve.</param>
        /// <param name="section">The settings in name, value format.</param>
        /// <returns>True if section could be retrieved, false otherwise.</returns>
        bool TryGetSection(string name, out IEnumerable<KeyValuePair<string, string>> section);
    }
}
