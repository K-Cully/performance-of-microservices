using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ClusterEmulator.Service.Simulation.Steps
{
    /// <summary>
    /// A step that simulates errors with a set probability.
    /// </summary>
    [Serializable]
    public class ErrorStep : SimulationStep
    {
        /// <summary>
        /// The probability of returning an error.
        /// </summary>
        [JsonProperty("probability")]
        [JsonRequired]
        [Range(0.0d, 1.0d, ErrorMessage = "probability must be in the range 0 to 1")]
        public double Probability { get; set; }


        /// <summary>
        /// Executes the action defined by the step.
        /// </summary>
        /// <returns><see cref="ExecutionStatus.Success"/> or <see cref="ExecutionStatus.SimulatedFail"/></returns>
        public async override Task<ExecutionStatus> ExecuteAsync()
        {
            if (Logger is null)
            {
                throw new InvalidOperationException("Logger is not initialized");
            }

            if (Probability < 0.0d || Probability > 1.0d)
            {
                Logger.LogCritical("{Property} value is not valid", "probability");
                throw new InvalidOperationException("probability must be in the range 0 to 1");
            }

            double value = new Random().NextDouble();
            ExecutionStatus status = value > Probability ? ExecutionStatus.Success : ExecutionStatus.SimulatedFail;

            Logger.LogDebug("{RandomValue} resulted in {ExecutionStatus} for {Probability}", value, status, Probability);
            return await Task.FromResult(status);
        }
    }
}
