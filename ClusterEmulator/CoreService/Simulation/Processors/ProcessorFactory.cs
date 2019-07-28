using Newtonsoft.Json;

namespace CoreService.Simulation.Processors
{
    /// <summary>
    /// Creates processor instances.
    /// </summary>
    public class ProcessorFactory : IProcessorFactory
    {
        /// <summary>
        /// Creates a concrete step object from a setting value.
        /// </summary>
        /// <param name="settingValue">The processor setting value.</param>
        /// <returns>An initialized <see cref="IProcessor"/> instance.</returns>
        /// <remarks>
        /// Expected setting form:
        /// { <object>
        /// </remarks>
        public IProcessor Create(string settingValue)
        {
            // TODO: log & handle deserialization errors
            return JsonConvert.DeserializeObject<Processor>(settingValue);
        }
    }
}
