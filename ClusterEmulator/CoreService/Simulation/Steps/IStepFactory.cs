namespace CoreService.Simulation.Steps
{
    /// <summary>
    /// Creates step instances.
    /// </summary>
    public interface IStepFactory
    {
        /// <summary>
        /// Creates a concrete step object from a setting value.
        /// </summary>
        /// <param name="settingValue">The step setting value.</param>
        /// <returns>An initialized <see cref="IStep"/> instance.</returns>
        IStep Create(dynamic settingValue);
    }
}