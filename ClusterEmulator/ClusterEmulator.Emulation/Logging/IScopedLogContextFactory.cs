using Microsoft.AspNetCore.Http;
using System;

namespace ClusterEmulator.Emulation.Logging
{
    /// <summary>
    /// A factory for creation of scoped logging contexts.
    /// </summary>
    public interface IScopedLogContextFactory
    {
        /// <summary>
        /// Initializes a scoped logging context based off the provided http context.
        /// </summary>
        /// <param name="httpContext">The current http context.</param>
        /// <returns>A scoped logging context to be disposed from the calling thread.</returns>
        IDisposable InitializeFrom(HttpContext httpContext);
    }
}
