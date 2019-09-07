using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CoreService.Simulation.Steps
{
    /// <summary>
    /// A step that simulates CPU bound operations and memory allocation.
    /// </summary>
    [Serializable]
    public class LoadStep : IStep
    {
        [JsonIgnore]
        private ILogger log;


        [JsonIgnore]
        private ILogger Logger { get => log; set => log = log ?? value; }


        /// <summary>
        /// The length of time the load should last for.
        /// </summary>
        [JsonProperty("time")]
        [JsonRequired]
        [Range(0, int.MaxValue, ErrorMessage = "time cannot be negative")]
        public int TimeInSeconds { get; set; }


        /// <summary>
        /// The percentage of processor time that should be consumed.
        /// </summary>
        [JsonProperty("percent")]
        [JsonRequired]
        [Range(1, 100, ErrorMessage = "percent must be in the range 1 - 100")]
        public int CpuPercentage { get; set; }


        /// <summary>
        /// The amount of memory to consume.
        /// </summary>
        [JsonProperty("bytes")]
        [JsonRequired]
        public ulong MemoryInBytes { get; set; }


        /// <summary>
        /// Executes the action defined by the step.
        /// </summary>
        /// <returns><see cref="ExecutionStatus.Success"/></returns>
        public async Task<ExecutionStatus> ExecuteAsync()
        {
            if (Logger is null)
            {
                throw new InvalidOperationException("Logger is not initialized");
            }

            if (TimeInSeconds < 0)
            {
                Logger.LogCritical("{Property} is negative", "time");
                throw new InvalidOperationException("time cannot be negative");
            }

            if (CpuPercentage < 1 || CpuPercentage > 100)
            {
                Logger.LogCritical("{Property} value is not in the accepted range", "percent");
                throw new InvalidOperationException("percent must be in the range 1 - 100");
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

            List<Task> coreTasks = new List<Task>();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                Logger.LogDebug("Generating {LoadPercent}% load on processor {ProcessorNumber} for {Time} seconds",
                    CpuPercentage, i, TimeInSeconds);
                coreTasks.Add(GenerateLoad(TimeInSeconds, CpuPercentage));
            }

            await Task.WhenAll(coreTasks).ConfigureAwait(false);
            log.LogInformation("Completed load generation step");
            return ExecutionStatus.Success;
        }


        /// <summary>
        /// Initializes a logger for the step instance.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        public void InitializeLogger(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        private async Task GenerateLoad(int seconds, int percentage)
        {
            DateTime start = DateTime.UtcNow;
            Stopwatch watch = new Stopwatch();

            watch.Start();
            while (seconds > DateTime.UtcNow.Subtract(start).Seconds)
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
