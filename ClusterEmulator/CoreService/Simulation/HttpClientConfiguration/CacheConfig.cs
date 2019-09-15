using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;

namespace CoreService.Simulation.HttpClientConfiguration
{
    public class CacheConfig : IPolicyConfiguration
    {
        /// <summary>
        /// Generates a Polly <see cref="CachePolicy{HttpResponseMessage}"/> from the configuration.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to use for logging.</param>
        /// <returns>A <see cref="CachePolicy{HttpResponseMessage}"/> instance.</returns>
        /// <remarks>
        /// Currently only supports in-memory cache.
        /// Operation key set on the context is used as the cache key.
        /// </remarks>
        public IAsyncPolicy<HttpResponseMessage> AsPolicy(ILogger logger)
        {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));


            // TODO: validate state


            // Create delegates
            void OnCacheGet(Context context, string key) =>
                logger.LogTrace("{PolicyKey} at {OperationKey}: Retrieving {Key} from cache",
                    context.PolicyKey, context.OperationKey, key);

            void OnCacheMiss(Context context, string key) =>
                logger.LogTrace("{PolicyKey} at {OperationKey}: {Key} was not present in cache",
                    context.PolicyKey, context.OperationKey, key);

            void OnCachePut(Context context, string key) => 
                logger.LogTrace("{PolicyKey} at {OperationKey}: Inserting {Key} into cache",
                    context.PolicyKey, context.OperationKey, key);

            void OnCacheGetError(Context context, string key, Exception exception) =>
                logger.LogError(exception, "{PolicyKey} at {OperationKey}: Error retrieving {Key} from cache",
                    context.PolicyKey, context.OperationKey, key);

            void OnCachePutError(Context context, string key, Exception exception) =>
                logger.LogError(exception, "{PolicyKey} at {OperationKey}: Error inserting {Key} into cache",
                    context.PolicyKey, context.OperationKey, key);

            // TODO: allow injection of cache
            // TODO: allow mocking
            // Create cache provider
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            IAsyncCacheProvider<HttpResponseMessage> cacheProvider =
                new MemoryCacheProvider(memoryCache).AsyncFor<HttpResponseMessage>();


            // TODO: update Request step to set cache key

            // TODO: define properties

            // TODO: handle omly status codes that should be cached

            // Create policy
            var cache = Policy
                .CacheAsync(cacheProvider,
                    ttl: TimeSpan.FromMinutes(5.0d), // | ITtlStrategy ttlStrategy
                    cacheKeyStrategy: new DefaultCacheKeyStrategy(), // Is this required? | Func<Context, string> cacheKeyStrategy]
                    onCacheGet: OnCacheGet,
                    onCacheMiss: OnCacheMiss,
                    onCachePut: OnCachePut,
                    onCacheGetError: OnCacheGetError,
                    onCachePutError: OnCachePutError);

            return cache;
        }
    }
}
