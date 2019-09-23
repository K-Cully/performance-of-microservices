using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace NameGeneratorService.Core
{
    /// <summary>
    /// Processes requests to generate names
    /// </summary>
    public class NameProcessor : INameProcessor
    {
        private IHttpClientFactory m_clientFactory;

        /// <summary>
        /// Initializes new instances of <see cref="NameProcessor"/>
        /// </summary>
        /// <param name="clientFactory">The <see cref="IHttpClientFactory"/> instance to resolve clients from</param>
        public NameProcessor(IHttpClientFactory clientFactory)
        {
            m_clientFactory = clientFactory;
        }


        /// <summary>
        /// Generates the specified number of names.
        /// </summary>
        /// <param name="count">The number of names to generate.</param>
        /// <returns>An enumeration of names.</returns>
        public async Task<IEnumerable<string>> GenerateNamesAsync(int count)
        {
            using (HttpClient client = m_clientFactory.CreateClient(Settings.RandomApiClientName))
            {
                // TODO: await all requests
                // TODO: send correct requests
                // TODO: deal with response model correctly
                await client.GetAsync("/");
            }

            using (HttpClient client = m_clientFactory.CreateClient(Settings.NameLookupApiClientName))
            {
                // TODO: send correct requests
                // TODO: deal with response model correctly
            }

            // TODO: complete
            return new List<string>(count);
        }
    }
}
