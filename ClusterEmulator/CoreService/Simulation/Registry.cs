using CoreService.Simulation.Steps;
using System;
using System.Collections.Generic;
using System.Fabric.Description;

namespace CoreService.Simulation
{
    // TODO: refactor duplicate logic, add comments and tests

    public class Registry : IRegistry
    {
        private readonly ConfigurationSettings settings;


        private IDictionary<string, IProcessor> Processors { get; set; }


        private IDictionary<string, IStep> Steps { get; set; }


        public Registry(ConfigurationSettings configurationSettings)
        {
            settings = configurationSettings ??
                throw new ArgumentNullException(nameof(configurationSettings));

            Processors = new Dictionary<string, IProcessor>();
            foreach (var property in settings.Sections["Processors"].Parameters)
            {
                // TODO: parse processor from value
                Processors.Add(property.Name, new Processor());
            }

            // TODO: load steps
            Steps = new Dictionary<string, IStep>();
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
    }
}
