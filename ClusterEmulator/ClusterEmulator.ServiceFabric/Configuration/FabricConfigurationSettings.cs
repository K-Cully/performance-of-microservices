using System;
using System.Collections.Generic;
using System.Fabric.Description;
using System.Linq;
using ClusterEmulator.Emulation.Core;

namespace ClusterEmulator.Service.Shared.Configuration
{
    public class FabricConfigurationSettings : IRegistrySettings
    {
        private readonly ConfigurationSettings settings;


        /// <summary>
        /// Creates an instance of <see cref="FabricConfigurationSettings"/>
        /// </summary>
        /// <param name="serviceSettings">Configuration settings loaded from the service settings.xml file</param>
        public FabricConfigurationSettings(ConfigurationSettings serviceSettings)
        {
            settings = serviceSettings ?? throw new ArgumentNullException(nameof(serviceSettings));
        }


        /// <summary>
        /// Attempts to retrieve the enumeration of settings from a configuration section
        /// </summary>
        /// <param name="name">The name of the section to retrieve.</param>
        /// <param name="section">The settings in name, value format.</param>
        /// <returns>True if section could be retrieved, false otherwise.</returns>
        public bool TryGetSection(string name, out IEnumerable<KeyValuePair<string, string>> section)
        {
            section = null;
            if (name is null || settings.Sections is null)
            {
                return false;
            }

            if (!settings.Sections.TryGetValue(name, out ConfigurationSection configurationSection))
            {
                return false;
            }

            section = configurationSection.Parameters
                .Select(p => new KeyValuePair<string, string>(p.Name, p.Value))
                .ToList();
            return true;
        }
    }
}
