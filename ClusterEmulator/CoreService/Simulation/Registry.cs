﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreService.Simulation
{
    public class Registry : IRegistry
    {
        public Registry()
        {
            // TODO: load settings
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


        private IDictionary<string, IProcessor> Processors { get; set; }


        private IDictionary<string, IStep> Steps { get; set; }
    }
}
