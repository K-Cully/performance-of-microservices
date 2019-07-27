using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CoreService.Simulation.Steps
{
    /// <summary>
    /// A step that simulates CPU bound operations.
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
        /// Executes the action defined by the step.
        /// </summary>
        /// <returns><see cref="ExecutionStatus.Success"/></returns>
        public async Task<ExecutionStatus> ExecuteAsync()
        {
            // TODO

            // TODO: validate and get from settings
            int seconds = 5;
            int percentage = 50;

            List<Task> coreTasks = new List<Task>();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                // TODO: support cancellation
                coreTasks.Add(GenerateLoad(seconds, percentage));
            }

            await Task.WhenAll(coreTasks).ConfigureAwait(false);
            return ExecutionStatus.Success;
        }


        private async Task GenerateLoad(int seconds, int percentage)
        {
            // TODO: fix
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
