using Microsoft.Extensions.Logging;
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

        private IHttpClientFactory ClientFactory { get; }

        private ILogger Logger { get; }


        /// <summary>
        /// Initializes new instances of <see cref="NameProcessor"/>
        /// </summary>
        /// <param name="clientFactory">The <see cref="IHttpClientFactory"/> instance to resolve clients from</param>
        /// <param name="logger">The application trace logger</param>
        public NameProcessor(IHttpClientFactory clientFactory, ILogger<NameProcessor> logger)
        {
            ClientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        /// Generates a single name.
        /// </summary>
        /// <returns>The generated name or empty.</returns>
        public async Task<string> GenerateNameAsync()
        {
            return (await GenerateNamesAsync(1).ConfigureAwait(false)).FirstOrDefault();
        }


        /// <summary>
        /// Generates the specified number of names.
        /// </summary>
        /// <param name="count">The number of names to generate.</param>
        /// <returns>An enumeration of names or null if an error occurred.</returns>
        public async Task<IEnumerable<string>> GenerateNamesAsync(int count)
        {
            IEnumerable<int> ids = await GetRandomIds(count).ConfigureAwait(false);

            if (ids is null || ids.Count() == 0)
            {
                Logger.LogWarning("{Operation} failed. Ids were not generated, retuning null.",
                    nameof(GetRandomIds));
                return null;
            }

            Logger.LogDebug("Retrieving {NameCount} names with ids: {NameIDs}", count, ids);
            using (HttpClient client = ClientFactory.CreateClient(Settings.NameLookupApiClientName))
            {
                if (count == 1)
                {
                    return await RetrieveName(ids.First(), client);
                }
                else
                {
                    return await RetrieveNames(ids, client);
                }
            }
        }


        private async Task<IEnumerable<string>> RetrieveName(int id, HttpClient client)
        {
            using (HttpResponseMessage response =
                await client.GetAsync($"{client.BaseAddress?.AbsolutePath}/api/lookup/{id}").ConfigureAwait(false))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Logger.LogError("GET request for {NameID} failed with status code {StatusCode}",
                        id, response.StatusCode);
                    return null;
                }

                return await response.Content.ReadAsAsync<IEnumerable<string>>().ConfigureAwait(false);
            }
        }


        private async Task<IEnumerable<string>> RetrieveNames(IEnumerable<int> ids, HttpClient client)
        {
            using (HttpResponseMessage response =
                await client.PostAsJsonAsync($"{client.BaseAddress?.AbsolutePath}/api/lookup", ids).ConfigureAwait(false))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Logger.LogError("POST request for {NameIDs} failed with status code {StatusCode}",
                        ids, response.StatusCode);
                    return null;
                }

                return await response.Content.ReadAsAsync<IEnumerable<string>>().ConfigureAwait(false);
            }
        }


        private async Task<IEnumerable<int>> GetRandomIds(int count)
        {
            var randomTasks = new List<Task<HttpResponseMessage>>();

            try
            {
                Logger.LogDebug("Generating {RandomCount} random numbers", count);
                using (HttpClient client = ClientFactory.CreateClient(Settings.RandomApiClientName))
                {
                    for (int i = 0; i < count; i++)
                    {
                        randomTasks.Add(client.GetAsync($"{client.BaseAddress?.AbsolutePath}/api/random/{maximumId}"));
                    }

                    await Task.WhenAll(randomTasks).ConfigureAwait(false);
                    if (randomTasks.Any(t => t.Result.StatusCode != HttpStatusCode.OK))
                    {
                        // Current flow is to fail out if any calls fail
                        Logger.LogError("An error occurred while generating {IdCount} random IDs, returning null.", count);
                        return null;
                    }

                    Logger.LogInformation("Successfully generated {IdCount} IDs.", count);
                    return randomTasks.Select(t => t.Result.Content.ReadAsAsync<int>().Result).ToList();
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
