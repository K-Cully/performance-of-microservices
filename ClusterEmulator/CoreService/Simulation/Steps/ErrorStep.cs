using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace CoreService.Simulation.Steps
{
    /// <summary>
    /// A step that simulates errors with a set probability.
    /// </summary>
    [Serializable]
    public class ErrorStep : IStep
    {
        /// <summary>
        /// The length of time the load should last for.
        /// </summary>
        [JsonProperty("probability")]
        [JsonRequired]
        [Range(0.0d, 1.0d, ErrorMessage = "probability must be in the range 0 to 1")]
        public double Probability { get; set; }


        /// <summary>
        /// Executes the action defined by the step.
        /// </summary>
        /// <returns><see cref="ExecutionStatus.Success"/> or <see cref="ExecutionStatus.Fail"/></returns>
        public async Task<ExecutionStatus> ExecuteAsync()
        {
            if (Probability < 0.0d || Probability > 1.0d)
            {
                throw new InvalidOperationException("probability must be in the range 0 to 1");
            }

            Random random = new Random();
            return await Task.FromResult(random.NextDouble() > Probability ? ExecutionStatus.Success : ExecutionStatus.Fail);
        }
    }
}
