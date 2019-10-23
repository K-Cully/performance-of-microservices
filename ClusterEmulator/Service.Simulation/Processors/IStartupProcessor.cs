namespace ClusterEmulator.Service.Simulation.Processors
{
    /// <summary>
    /// An interface to define startup processing configuration.
    /// </summary>
    public interface IStartupProcessor : IProcessor
    {
        /// <summary>
        /// A value indicating whether the processor should execute asynchronously or not.
        /// </summary>
        bool Asynchronous { get; set; }
    }
}