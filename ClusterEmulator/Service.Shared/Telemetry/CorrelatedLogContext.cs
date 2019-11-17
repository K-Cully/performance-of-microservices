using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog.Context;
using System;
using System.Linq;

namespace ClusterEmulator.Service.Shared.Telemetry
{
    /// <summary>
    /// Creates a log context with a correlation id.
    /// </summary>
    public class CorrelatedLogContext : IScopedLogContextFactory
    {
        /// <summary>
        /// Pushes a corerlation id onto the context, returning an System.IDisposable that must later
        /// be used to remove the property, along with any others that may have been pushed
        /// on top of it and not yet popped. The property must be popped from the same thread/logical
        /// call context.
        /// </summary>
        /// <remarks>Uses the correlation header if available, generates a new id otherwise.</remarks>
        /// <param name="httpContext">The current http context.</param>
        /// <returns>The a disposable object that must be used to remove the property from the log context.</returns>
        public IDisposable InitializeFrom(HttpContext httpContext)
        {
            _ = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            _ = httpContext.Request?.Headers ?? throw new ArgumentException("Headers are not initialized", nameof(httpContext));

            string correlationId = httpContext.Request.Headers.TryGetValue("Request-Id", out StringValues correlationHeaders) ?
                correlationHeaders.FirstOrDefault() : null;

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            return LogContext.PushProperty("CorrelationId", correlationId);
        }
    }
}
