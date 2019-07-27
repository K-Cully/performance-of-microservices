using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Simulation.Steps
{
    public class LoadStep : IStep
    {
        // TODO

        public async Task<ExecutionStatus> ExecuteAsync()
        {
            // TODO

            // TODO: validate and get from settings
            int seconds = 5;
            int percentage = 50;

            List<Task> coreTasks = new List<Task>();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
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
