using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NameGeneratorService.Core
{
    /// <summary>
    /// Processes requests to generate names
    /// </summary>
    public class NameProcessor : INameProcessor
    {
        private const int maximumId = 2200;


        private readonly IHttpClientFactory m_clientFactory;


        /// <summary>
        /// Initializes new instances of <see cref="NameProcessor"/>
        /// </summary>
        /// <param name="clientFactory">The <see cref="IHttpClientFactory"/> instance to resolve clients from</param>
        public NameProcessor(IHttpClientFactory clientFactory)
        {
            m_clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }


        /// <summary>
        /// Generates the specified number of names.
        /// </summary>
        /// <param name="count">The number of names to generate.</param>
        /// <returns>An enumeration of names.</returns>
        public async Task<IEnumerable<string>> GenerateNamesAsync(int count)
        {
            var randomTasks = new List<Task<HttpResponseMessage>>();
            try
            {
                IEnumerable<int> ids;

                using (HttpClient client = m_clientFactory.CreateClient(Settings.RandomApiClientName))
                {
                    for (int i = 0; i < count; i++)
                    {
                        randomTasks.Add(client.GetAsync($"/api/random/{maximumId}"));
                    }

                    await Task.WhenAll(randomTasks).ConfigureAwait(false);
                    if (randomTasks.Any(t => t.Result.StatusCode != HttpStatusCode.OK))
                    {
                        // TODO: log
                        return new List<string>();
                    }

                    ids = randomTasks.Select(t => t.Result.Content.ReadAsAsync<int>().Result);
                }

                using (HttpClient client = m_clientFactory.CreateClient(Settings.NameLookupApiClientName))
                {
                    using (HttpResponseMessage response = 
                        await client.PostAsJsonAsync("/api/lookup", ids).ConfigureAwait(false))
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            // TODO: log
                            return new List<string>();
                        }

                        return await response.Content.ReadAsAsync<IEnumerable<string>>().ConfigureAwait(false);

                        // TODO: deal with missing values?
                    }
                }
            }
            finally
            {
                foreach (var task in randomTasks)
                {
                    task?.Result?.Dispose();
                }
            }
        }
    }
}
