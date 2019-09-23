using System.Collections.Generic;

namespace NameGeneratorService.Core
{
    /// <summary>
    /// Processes requests to generate names
    /// </summary>
    public class NameProcessor : INameProcessor
    {
        /// <summary>
        /// Generates the specified number of names.
        /// </summary>
        /// <param name="count">The number of names to generate.</param>
        /// <returns>An enumeration of names.</returns>
        public IEnumerable<string> GenerateNames(int count)
        {
            // TODO: complete
            return new List<string>(count);
        }
    }
}
