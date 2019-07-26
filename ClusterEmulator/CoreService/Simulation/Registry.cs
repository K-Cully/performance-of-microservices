using CoreService.Simulation.Steps;
using System;
using System.Collections.Generic;
using System.Fabric.Description;

namespace CoreService.Simulation
{
    public class Registry : IRegistry
    {
        private ConfigurationSettings Settings { get; set; }


        private IDictionary<string, IProcessor> Processors { get; set; }


        private IDictionary<string, IStep> Steps { get; set; }


        public Registry(ConfigurationSettings configurationSettings)
        {
            Settings = configurationSettings ??
                throw new ArgumentNullException(nameof(configurationSettings));

            // TODO: load processors and steps into cache
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

            // TODO: parse value into processor
            string value = Settings.Sections["Processors"].Parameters[name].Value;

            // TODO use cache

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

            string value = Settings.Sections["Processors"].Parameters[name].Value;

            return step;
        }
    }
}
