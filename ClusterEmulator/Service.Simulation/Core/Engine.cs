﻿using ClusterEmulator.Service.Simulation.Processors;
using ClusterEmulator.Service.Simulation.Steps;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ClusterEmulator.Service.Simulation.Core
{
    /// <summary>
    /// Loads and executes emulated and simulated request components.
    /// </summary>
    public class Engine : IEngine
    {
        private readonly IRegistry registry;
        private readonly ILogger<Engine> log;


        /// <summary>
        /// Initializes a new instance of <see cref="Engine"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <param name="simulationRegistry">The simulation registry to load content from.</param>
        public Engine(ILogger<Engine> logger, IRegistry simulationRegistry)
        {
            log = logger ?? throw new ArgumentNullException(nameof(logger));
            registry = simulationRegistry ?? throw new ArgumentNullException(nameof(simulationRegistry));
        }


        /// <summary>
        /// Emulates the processing of the request based on the named configuration.
        /// </summary>
        /// <param name="name">The name of the processor configuration.</param>
        /// <returns>
        /// An <see cref="OkObjectResult"/> if all steps successful.
        /// An <see cref="ObjectResult"/> with Status418ImATeapot when configured to simulate an error.
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
                ExecutionStatus status = await step.ExecuteAsync().ConfigureAwait(false);

                switch (status)
                {
                    case ExecutionStatus.Success:
                        log.LogInformation("{Step} completed with success in {Processor}", stepName, name);
                        continue;
                    case ExecutionStatus.SimulatedFail:
                        log.LogInformation("{Step} completed with simulated error in {Processor}", stepName, name);
                        errorResult.StatusCode =  StatusCodes.Status418ImATeapot;
                        return errorResult;
                    case ExecutionStatus.Fail:
                    default:
                        log.LogError("{Step} experienced an unexpected error in {Processor}", stepName, name);
                        errorResult.StatusCode = StatusCodes.Status500InternalServerError;
                        return errorResult;
                }
            }

            return new OkObjectResult(processor.SuccessPayload);
        }
    }
}