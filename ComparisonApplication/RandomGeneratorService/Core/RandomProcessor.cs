using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace RandomGeneratorService.Core
{
    /// <summary>
    /// Processes requests for random neumbers
    /// </summary>
    public class RandomProcessor : IRandomProcessor
    {
        private ISeedGenerator SeedGenerator { get; }

        private ILogger Logger { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="RandomProcessor"/>
        /// </summary>
        /// <param name="seedGenerator">The seed generator to use</param>
        /// <param name="logger">The application trace logger</param>
        public RandomProcessor(ISeedGenerator seedGenerator, ILogger<RandomProcessor> logger)
        {
            SeedGenerator = seedGenerator ?? throw new ArgumentNullException(nameof(seedGenerator));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        /// Retrieves a random number using generated seed values
        /// </summary>
        /// <param name="max">The maximum number to return</param>
        /// <returns>A positive random integer</returns>
        public async Task<int> GetRandomNumber(int max)
        {
            uint seed = await Task.FromResult(SeedGenerator.Generate())
                .ConfigureAwait(false);
            Logger.LogInformation("Seed generated: {Seed}", seed);

            int random = new Random((int)seed).Next(max);
            Logger.LogDebug("Random value {RandomValue} generated with maximum value {RandomMax}",
                random, max);

            return random;
        }
    }
}
