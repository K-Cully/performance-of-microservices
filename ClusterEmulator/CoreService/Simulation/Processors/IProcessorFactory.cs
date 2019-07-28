namespace CoreService.Simulation.Processors
{
    /// <summary>
    /// Creates processor instances.
    /// </summary>
    public interface IProcessorFactory
    {
        /// <summary>
        /// Creates a concrete step object from a setting value.
        /// </summary>
        /// <param name="settingValue">The processor setting value.</param>
        /// <returns>An initialized <see cref="IProcessor"/> instance.</returns>
        IProcessor Create(string settingValue);
    }
}