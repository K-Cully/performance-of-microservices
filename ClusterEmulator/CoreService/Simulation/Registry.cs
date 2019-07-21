using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreService.Simulation
{
    public class Registry
    {
        public Registry()
        {
            // TODO: load settings
        }


        public IProcessor GetProcessor(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name), "Processor name cannot be null");
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
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name), "Step name cannot be null");
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
