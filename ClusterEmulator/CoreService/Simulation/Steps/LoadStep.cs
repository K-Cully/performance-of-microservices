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
        /// The percentage of processor time that should be consumed.
        /// </summary>
        [JsonProperty("bytes")]
        [JsonRequired]
        [Range(0, int.MaxValue, ErrorMessage = "bytes must be in the range 0 - 2,147,483,647")]
        public int MemoryInBytes { get; set; }


        /// <summary>
        /// Executes the action defined by the step.
        /// </summary>
        /// <returns><see cref="ExecutionStatus.Success"/></returns>
        public async Task<ExecutionStatus> ExecuteAsync()
        {
            if (TimeInSeconds < 0)
            {
                throw new InvalidOperationException("time cannot be negative");
            }

            if (CpuPercentage < 1 || CpuPercentage > 100)
            {
                throw new InvalidOperationException("percent must be in the range 1 - 100");
            }

            if (MemoryInBytes < 0)
            {
                throw new InvalidOperationException("bytes must be in the range 0 - 2,147,483,647");
            }

            char[] temporary = null;
            if (MemoryInBytes > 0)
            {
                temporary = new char[(MemoryInBytes / 2) + 1];
            }

            List<Task> coreTasks = new List<Task>();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                coreTasks.Add(GenerateLoad(TimeInSeconds, CpuPercentage));
            }

            await Task.WhenAll(coreTasks).ConfigureAwait(false);
            if (temporary != null)
            {
                // Using temporary to avoid compiler optimization removing it
                temporary[0] = 'P';
            }

            return ExecutionStatus.Success;
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
