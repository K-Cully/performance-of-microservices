using CoreService.Simulation.Processors;
using CoreService.Simulation.Steps;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CoreService.Simulation.Core
{
    /// <summary>
    /// Loads and executes emulated and simulated request components.
    /// </summary>
    public class Engine : IEngine
    {
        private readonly IRegistry registry;


        /// <summary>
        /// Initializes a new instance of <see cref="Engine"/>.
        /// </summary>
        /// <param name="simulationRegistry">The simulation registry to load content from.</param>
        public Engine(IRegistry simulationRegistry)
        {
            registry = simulationRegistry ?? throw new ArgumentNullException(nameof(simulationRegistry));
        }


        /// <summary>
        /// Emulates the processing of the request based on the named configuration.
        /// </summary>
        /// <param name="name">The name of the processor configuration.</param>
        /// <returns>
        /// An <see cref="OkObjectResult"/> if all steps successful.
        /// An <see cref="ObjectResult"/> with Status404NotFound when configured to simulate an error.
        /// An <see cref="ObjectResult"/> with Status500InternalServerError if an unexpected error occurs.
        /// </returns>
        public async Task<IActionResult> ProcessRequestAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"{nameof(name)} cannot be null or whitespace", nameof(name));
            }

            IProcessor processor = registry.GetProcessor(name);
            await Task.Delay(processor.IngressLatencyMilliseconds);

            ObjectResult errorResult = new ObjectResult(processor.ErrorPayload);
            foreach (string stepName in processor.Steps)
            {
                IStep step = registry.GetStep(stepName);
                ExecutionStatus status = await step.ExecuteAsync();

                switch (status)
                {
                    case ExecutionStatus.Success:
                        continue;
                    case ExecutionStatus.Fail:
                        errorResult.StatusCode = StatusCodes.Status404NotFound;
                        return errorResult;
                    case ExecutionStatus.Unexpected:
                    default:
                        errorResult.StatusCode = StatusCodes.Status500InternalServerError;
                        return errorResult;
                }
            }

            return new OkObjectResult(processor.SuccessPayload);
        }
    }
}