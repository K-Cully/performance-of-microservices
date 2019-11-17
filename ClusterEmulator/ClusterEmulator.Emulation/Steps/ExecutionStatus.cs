namespace ClusterEmulator.Emulation.Steps
{
    /// <summary>
    /// Indicates the outcome of an execution step.
    /// </summary>
    public enum ExecutionStatus
    {
        /// <summary>
        /// An unexpected error has occurred.
        /// </summary>
        Fail = 0,


        /// <summary>
        /// Step execution completed and the caller should return an error.
        /// </summary>
        SimulatedFail = 2,


        /// <summary>
        /// Step execution completed and further steps should proceed.
        /// </summary>
        Success = 1,
    }
}
