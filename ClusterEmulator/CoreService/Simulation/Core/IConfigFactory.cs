namespace CoreService.Simulation.Core
{
    /// <summary>
    /// Creates concrete instances of the specifed type.
    /// </summary>
    /// <typeparam name="TModel">The type to generate from setting values.</typeparam>
    public interface IConfigFactory<TModel>
        where TModel : class
    {
        /// <summary>
        /// Creates a concrete object from a setting value.
        /// </summary>
        /// <param name="settingValue">The setting value.</param>
        /// <returns>An initialized <see cref="TModel"/> instance.</returns>
        TModel Create(string settingValue);
    }
}