using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        public int TimeInSeconds { get; set; }


        /// <summary>
        /// The percentage of processor time that should be consumed.
        /// </summary>
        [JsonProperty("percent")]
        [JsonRequired]
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

            await Task.WhenAll(coreTasks);
            return ExecutionStatus.Success;
        }


        private async Task GenerateLoad(int seconds, int percentage)
        {
            Stopwatch watch = new Stopwatch();

            long runtime = (long)seconds * 1000;
            long elapsed = 0;
            watch.Start();
            while (elapsed < runtime)
            {
                // Generate load for the target percentage, sleep for the remaining time
                if (watch.ElapsedMilliseconds > percentage)
                {
                    elapsed += watch.ElapsedMilliseconds;
                    await Task.Delay(100 - percentage).ConfigureAwait(false);
                    watch.Reset();
                    watch.Start();
                }
            }
        }
    }
}
