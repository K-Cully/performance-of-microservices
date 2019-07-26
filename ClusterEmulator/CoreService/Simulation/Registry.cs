using CoreService.Simulation.Steps;
using System;
using System.Collections.Generic;
using System.Fabric.Description;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace CoreService.Simulation
{
    public class Registry : IRegistry
    {
        private ConfigurationSettings Settings { get; set; }


        public Registry(ConfigurationSettings configurationSettings)
        {
            Settings = configurationSettings ??
                throw new ArgumentNullException(nameof(configurationSettings));
        }


        public IProcessor GetProcessor(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Processor name cannot be null or whitespace", nameof(name));
            }

            if (!Processors.TryGetValue(name, out IProcessor processor))
            {
                throw new InvalidOperationException($"Processor '{name}' is not registered");
            }

            if (processor == null)
            {
                throw new InvalidOperationException($"Registration for processor '{name}' is null");
            }


            // TODO: Settings.Sections["Processors"].Parameters["Procesor_1"];

            return processor;
        }


        public IStep GetStep(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Step name cannot be null or whitespace", nameof(name));
            }

            if (!Steps.TryGetValue(name, out IStep step))
            {
                throw new InvalidOperationException($"Step '{name}' is not registered");
            }

            if (step == null)
            {
                throw new InvalidOperationException($"Registration for step '{name}' is null");
            }

            return step;
        }


        private IDictionary<string, IProcessor> Processors { get; set; }


        private IDictionary<string, IStep> Steps { get; set; }
    }
}
