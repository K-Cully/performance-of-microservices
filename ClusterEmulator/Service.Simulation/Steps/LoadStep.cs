using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ClusterEmulator.Service.Simulation.Steps
{
    /// <summary>
    /// A step that simulates CPU bound operations and memory allocation.
    /// </summary>
    public class LoadStep : SimulationStep
    {
        /// <summary>
        /// The length of time the load should last for.
        /// </summary>
        /// <remarks>A negative value indicates run forever</remarks>
        [JsonProperty("time")]
        [JsonRequired]
        public double TimeInSeconds { get; set; }


        /// <summary>
        /// The percentage of processor time that should be consumed.
        /// </summary>
        [JsonProperty("percent")]
        [JsonRequired]
        [Range(0, 100, ErrorMessage = "percent must be in the range 0 - 100")]
        public int CpuPercentage { get; set; }


        /// <summary>
        /// The maximum number of processors to load.
        /// </summary>
        /// <remarks>
        /// Used to restrict concurrency.
        /// If 0 or more than the number of available processors is specified, the number of processers available are loaded.
        /// </remarks>
        [JsonProperty("processors")]
        [Range(0, int.MaxValue, ErrorMessage = "processors cannot be negative")]
        public int MaxProcessors { get; set; }


        /// <summary>
        /// The amount of memory to consume.
        /// </summary>
        [JsonProperty("bytes")]
        [JsonRequired]
        public ulong MemoryInBytes { get; set; }


        [JsonIgnore]
        private int? processorCount;

        [JsonIgnore]
        private int ProcessorCount
        {
            get
            {
                if (processorCount == null)
                {
                    processorCount = MaxProcessors > 0 && MaxProcessors < Environment.ProcessorCount ?
                        MaxProcessors : Environment.ProcessorCount;
                }

                return processorCount.Value;
            }
        }


        /// <summary>
        /// Executes the action defined by the step.
        /// </summary>
        /// <returns><see cref="ExecutionStatus.Success"/></returns>
        public async override Task<ExecutionStatus> ExecuteAsync()
        {
            if (Logger is null)
            {
                throw new InvalidOperationException("Logger is not initialized");
            }

            if (CpuPercentage < 0 || CpuPercentage > 100)
            {
                Logger.LogCritical("{Property} value is not in the accepted range", "percent");
                throw new InvalidOperationException("percent must be in the range 0 - 100");
            }

            if (MaxProcessors < 0)
            {
                Logger.LogCritical("{Property} is negative", "processors");
                throw new InvalidOperationException("processors cannot be negative");
            }

            List<List<byte>> block = new List<List<byte>>();
            if (MemoryInBytes > 0)
            {
                ulong remaining = MemoryInBytes;
                Logger.LogDebug("Allocating {MemoryAllocation} bytes of total memory", remaining);

                while (remaining > int.MaxValue)
                {
                    Logger.LogDebug("Allocating chunk {MemoryAllocation} bytes of memory", int.MaxValue);
                    block.Add(new List<byte>(new byte[int.MaxValue]));
                    remaining -= int.MaxValue;
                }

                if (remaining > 0)
                {
                    Logger.LogDebug("Allocating chunk {MemoryAllocation} bytes of memory", (int)remaining);
                    block.Add(new List<byte>(new byte[(int)remaining]));
                }
            }

            if (TimeInSeconds == 0.0d)
            {
                Logger.LogInformation("Completed load generation step");
                return ExecutionStatus.Success;
            }

            if (CpuPercentage > 0)
            {
                // Generate CPU load
                List<Task> coreTasks = new List<Task>();
                for (int i = 0; i < ProcessorCount; i++)
                {
                    Logger.LogDebug("Generating {LoadPercent}% load on processor {ProcessorNumber} for {Time} seconds",
                        CpuPercentage, i, TimeInSeconds);
                    coreTasks.Add(GenerateLoad(TimeInSeconds, CpuPercentage));
                }

                await Task.WhenAll(coreTasks).ConfigureAwait(false);
            }
            else
            {
                // Utilize memory only, waiting forever if time is negative
                TimeSpan runTime = TimeInSeconds > 0.0d ?
                    TimeSpan.FromSeconds(TimeInSeconds) : TimeSpan.FromMilliseconds(-1.0d);
                await Task.Delay(runTime);
            }

            Logger.LogInformation("Completed load generation step");
            return ExecutionStatus.Success;
        }


        private async Task GenerateLoad(double seconds, int percentage)
        {
            DateTime start = DateTime.UtcNow;
            Stopwatch watch = new Stopwatch();

            // Run for the set time or forever if time is negative
            watch.Start();
            while (seconds < 0.0d || seconds > DateTime.UtcNow.Subtract(start).TotalSeconds)
            {
                // Generate load for the target percentage, sleep for the remaining time
                if (watch.ElapsedMilliseconds > percentage)
                {
                    await Task.Delay(100 - percentage).ConfigureAwait(false);
                    watch.Reset();
                    watch.Start();
                }
            }
        }
    }
}
