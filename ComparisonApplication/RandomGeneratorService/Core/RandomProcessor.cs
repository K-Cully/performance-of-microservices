using System;
using System.Threading.Tasks;

namespace RandomGeneratorService.Core
{
    /// <summary>
    /// Processes requests for random neumbers
    /// </summary>
    public class RandomProcessor : IRandomProcessor
    {
        readonly ISeedGenerator m_seedGenerator;


        /// <summary>
        /// Initializes a new instance of <see cref="RandomProcessor"/>
        /// </summary>
        /// <param name="seedGenerator">The seed generator to use</param>
        public RandomProcessor(ISeedGenerator seedGenerator)
        {
            m_seedGenerator = seedGenerator ?? throw new ArgumentNullException(nameof(seedGenerator));
        }


        /// <summary>
        /// Retrieves a random number using generated seed values
        /// </summary>
        /// <returns>A positive random integer</returns>
        public async Task<int> GetRandomNumber()
        {
            uint seed = await Task.FromResult(m_seedGenerator.Generate())
                .ConfigureAwait(false);
            return new Random((int)seed).Next();
        }
    }
}
