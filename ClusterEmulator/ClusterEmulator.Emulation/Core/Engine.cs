using ClusterEmulator.Service.Simulation.Processors;
using ClusterEmulator.Service.Simulation.Steps;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClusterEmulator.Service.Simulation.Core
{
    /// <summary>
    /// Loads and executes emulated and simulated components.
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

            IRequestProcessor processor = registry.GetRequestProcessor(name);
            await Task.Delay(processor.IngressLatencyMilliseconds);

            ObjectResult errorResult = new ObjectResult(processor.ErrorPayload);
            foreach (string stepName in processor.Steps)
            {
                IStep step = registry.GetStep(stepName);
                ExecutionStatus status = ExecutionStatus.Fail;

                status = step.ParallelCount == null || step.ParallelCount < 2
                    ? await step.ExecuteAsync().ConfigureAwait(false)
                    : await ExcuteStepInParallel(name, stepName, step).ConfigureAwait(false);

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


        /// <summary>
        /// Emulates startup business logic by executing all registered startup procsssors.
        /// </summary>
        /// <returns>
        /// A task.
        /// </returns>
        public async Task ProcessStartupActionsAsync()
        {
            // TODO: use this

            List<IStartupProcessor> processors = registry.GetStartupProcessors().ToList();
            foreach (var processor in processors)
            {
                if (processor.Asynchronous)
                {
                    Task task = RunStartupSteps(processor.Steps, nameof(processor));
                    _ = task.ContinueWith(t =>
                    {
                        log.LogError("Asynchronous startup processor failed {Processor}", nameof(processor));
                        t.Exception.Handle(ex =>
                        {
                            // Log and set all exceptions as handled to avoid rethrowing
                            log.LogCritical(ex, "{Processor}: Exception returned from asynchronous startup processor", nameof(processor));
                            return true;
                        });
                    }, TaskContinuationOptions.OnlyOnFaulted);
                }
                else
                {
                    await RunStartupSteps(processor.Steps, nameof(processor)).ConfigureAwait(false);
                }
            }
        }


        private async Task RunStartupSteps(IList<string> steps, string processorName)
        {
            foreach (string stepName in steps)
            {
                IStep step = registry.GetStep(stepName);
                ExecutionStatus status = step.ParallelCount == null || step.ParallelCount < 2
                    ? await step.ExecuteAsync().ConfigureAwait(false)
                    : await ExcuteStepInParallel(processorName, stepName, step).ConfigureAwait(false);

                if (status != ExecutionStatus.Success)
                {
                    log.LogCritical("{Step} experienced an error in startup processor {Processor}",
                        stepName, processorName);
                    throw new InvalidOperationException($"Startup processor {processorName} encountered an error");
                }
            }
        }


        private async Task<ExecutionStatus> ExcuteStepInParallel(string name, string stepName, IStep step)
        {
            log.LogDebug("Executing {Step} {ParallelCount} times in parallel",
                stepName, step.ParallelCount);

            List<Task<ExecutionStatus>> tasks = new List<Task<ExecutionStatus>>();
            for (int count = 0; count < step.ParallelCount; count++)
            {
                tasks.Add(step.ExecuteAsync());
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            ExecutionStatus status = ExecutionStatus.Fail;
            switch (step.FailOnParallelFailures)
            {
                case GroupClause.All:
                    log.LogInformation("{Step} failure set to {GroupClause} in {Processor}, returning any success or first failure",
                        stepName, GroupClause.All, name);
                    status = tasks.FirstOrDefault(t => t.Result == ExecutionStatus.Success)?.Result ??
                        tasks.First(t => t.Result != ExecutionStatus.Success).Result;
                    break;
                case GroupClause.None:
                    log.LogInformation("{Step} failure set to {GroupClause} in {Processor}, returning success",
                        stepName, GroupClause.None, name);
                    status = ExecutionStatus.Success;
                    break;
                case GroupClause.Undefined:
                case GroupClause.Any:
                default:
                    log.LogInformation("{Step} failure set to {GroupClause} in {Processor}, returning first failure or success",
                        stepName, GroupClause.Any, name);
                    status = tasks.FirstOrDefault(t => t.Result != ExecutionStatus.Success)?.Result ?? ExecutionStatus.Success;
                    break;
            }

            return status;
        }
    }
}