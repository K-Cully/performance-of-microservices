using CoreService.Simulation.Processors;
using CoreService.Simulation.Steps;
using System;
using System.Collections.Generic;
using System.Fabric.Description;

namespace CoreService.Simulation.Core
{
    // TODO: add tests


    /// <summary>
    /// Handles registration and retrieval of simulation configuration settings for a service.
    /// </summary>
    public class Registry : IRegistry
    {
        private readonly ConfigurationSettings settings;


        private IDictionary<string, IProcessor> Processors { get; set; }


        private IDictionary<string, IStep> Steps { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="Registry"/>.
        /// </summary>
        /// <param name="configurationSettings">Service configuration settings from the service context.</param>
        /// <param name="stepFactory">A factory to create steps from settings.</param>
        /// <param name="processorFactory">A factory to create processors from settings.</param>
        public Registry(ConfigurationSettings configurationSettings, IStepFactory stepFactory, IProcessorFactory processorFactory)
        {
            settings = configurationSettings ??
                throw new ArgumentNullException(nameof(configurationSettings));

            Processors = new Dictionary<string, IProcessor>();
            foreach (var property in settings.Sections["Processors"].Parameters)
            {
                IProcessor processor = processorFactory.Create(property.Value);
                Processors.Add(property.Name, processor);
            }

            Steps = new Dictionary<string, IStep>();
            foreach (var property in settings.Sections["Steps"].Parameters)
            {
                IStep step = stepFactory.Create(property.Value);
                Steps.Add(property.Name, step);
            }
        }


        /// <summary>
        /// Retrieves the processor with a given name, if it is registered.
        /// </summary>
        /// <param name="name">The name of the processor.</param>
        /// <returns>The <see cref="IProcessor"/> instance.</returns>
        /// <exception cref="ArgumentException">
        /// name is null or white space.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The processor is not registered or the registration is not valid.
        /// </exception>
        public IProcessor GetProcessor(string name)
        {
            return GetRegisteredValue(name, Processors, "Processor");
        }


        /// <summary>
        /// Retrieves the step with a given name, if it is registered.
        /// </summary>
        /// <param name="name">The name of the step.</param>
        /// <returns>The <see cref="IStep"/> instance.</returns>
        /// <exception cref="ArgumentException">
        /// name is null or white space.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The step is not registered or the registration is not valid.
        /// </exception>
        public IStep GetStep(string name)
        {
            return GetRegisteredValue(name, Steps, "Step");
        }


        private T GetRegisteredValue<T>(string name, IDictionary<string, T> registry, string typeName)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"{typeName} name cannot be null or whitespace", nameof(name));
            }

            if (!registry.TryGetValue(name, out T value))
            {
                throw new InvalidOperationException($"{typeName} '{name}' is not registered");
            }

            if (value == null)
            {
                throw new InvalidOperationException($"Registration for {typeName} '{name}' is null");
            }

            return value;
        }
    }
}
