using Microsoft.Extensions.Logging;

namespace ClusterEmulator.Emulation.Core
{
    /// <summary>
    /// An interface for types representing config models that can be converted to an internal type model.
    /// </summary>
    /// <typeparam name="TModel">The type to create from the config model.</typeparam>
    public interface IConfigModel<TModel>
    {
        /// <summary>
        /// Converts the config model to the type model.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <returns>An instance of the <see cref="TModel"/> type.</returns>
        TModel AsTypeModel(ILogger logger);
    }
}
