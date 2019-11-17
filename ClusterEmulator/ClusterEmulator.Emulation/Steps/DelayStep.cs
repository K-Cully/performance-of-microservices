using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ClusterEmulator.Emulation.Steps
{
    /// <summary>
    /// A step that delays further processing for a set time.
    /// </summary>
    [Serializable]
    public class DelayStep : SimulationStep
    {
        /// <summary>
        /// The length of time the delay should last
        /// </summary>
        [JsonProperty("time")]
        [JsonRequired]
        [Range(0.0d, double.MaxValue, ErrorMessage = "time must not be negative")]
        public double Time { get; set; }


        [JsonIgnore]
        private TimeSpan Delay => TimeSpan.FromSeconds(Time);


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

            if (Time < 0.0d)
            {
                Logger.LogCritical("{Property} value is not valid", "time");
                throw new InvalidOperationException("time must not be negative");
            }

            await Task.Delay(Delay);
            return ExecutionStatus.Success;
        }
    }
}
