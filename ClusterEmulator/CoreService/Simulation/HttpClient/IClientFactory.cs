namespace CoreService.Simulation.HttpClient
{
    /// <summary>
    /// Creates client instances.
    /// </summary>
    public interface IClientFactory
    {
        /// <summary>
        /// Creates a concrete ClientConfig object from a setting value.
        /// </summary>
        /// <param name="settingValue">The client setting value.</param>
        /// <returns>An initialized <see cref="ClientConfig"/> instance.</returns>
        ClientConfig Create(string settingValue);
    }
}