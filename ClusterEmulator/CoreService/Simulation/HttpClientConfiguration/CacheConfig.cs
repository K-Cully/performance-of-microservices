using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;

namespace CoreService.Simulation.HttpClientConfiguration
{
    /// <summary>
    /// Configurable components of a cache policy.
    /// </summary>
    public class CacheConfig : IPolicyConfiguration
    {
        /// <summary>
        /// The cache invalidation time.
        /// </summary>
        /// <remarks>
        /// For absolute time caches, this time is added to 00:00 on the day a cache entry is created.
        /// When the time has already passed on cache emtry creation, a day is added to the absolute time.
        /// For all other cache expiration types, this is added the time of cache entry creation.
        /// </remarks>
        [JsonProperty("time", Required = Required.Always)]
        public CacheTime Time { get; set; }


        /// <summary>
        /// Whether the invalidation time should be treated as absolute or not.
        /// </summary>
        [JsonProperty("absoluteTime")]
        public bool Absolute { get; set; }


        /// <summary>
        /// Whether the invalidation time should be reset every time it is accessed or not.
        /// </summary>
        [JsonProperty("slidingExpiration")]
        public bool Sliding { get; set; }


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

            // TODO: update Request step to set cache key

            _ = logger ?? throw new ArgumentNullException(nameof(logger));

            if (Time is null || TimeSpan.Equals(Time.AsTimeSpan(), TimeSpan.Zero))
            {
                logger.LogCritical("{PolicyConfig} : {Property} must be a valid time span", nameof(CacheConfig), "time");
                throw new InvalidOperationException("time must be a valid time span");
            }

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

            if (!Absolute)
            {
                // Cache strategy if it is purely time span based 
                strategy = CreateStrategy();
            }

            // Only cache successful responses
            Ttl CacheOKResponse(Context context, HttpResponseMessage result) =>
                result.StatusCode == HttpStatusCode.OK ? Strategy.GetTtl(context, result) : new Ttl(TimeSpan.Zero);

            // Create policy with default cache key strategy
            var cache = Policy
                .CacheAsync(cacheProvider,
                    ttlStrategy: new ResultTtl<HttpResponseMessage>(CacheOKResponse),
                    onCacheGet: OnCacheGet,
                    onCacheMiss: OnCacheMiss,
                    onCachePut: OnCachePut,
                    onCacheGetError: OnCacheGetError,
                    onCachePutError: OnCachePutError);

            return cache;
        }


        private ITtlStrategy CreateStrategy()
        {
            TimeSpan cacheTime = Time.AsTimeSpan();
            if (Absolute)
            {
                DateTime invalidationTime = DateTimeOffset.Now.Date.Add(cacheTime);
                if (invalidationTime < DateTime.Now)
                {
                    // If time has already passed at time of cache entry creation, set at same time tomorrow
                    invalidationTime.AddDays(1);
                }

                return new AbsoluteTtl(invalidationTime);
            }

            if (Sliding)
            {
                return new SlidingTtl(cacheTime);
            }

            return new RelativeTtl(cacheTime);
        }


        [JsonIgnore]
        private ITtlStrategy strategy;


        /// <summary>
        /// Allows TTL strategy to be cached or computed on cache entry creation
        /// </summary>
        [JsonIgnore]
        private ITtlStrategy Strategy => strategy ?? CreateStrategy();
    }


    /// <summary>
    /// A model for the configuration's cache invalidation time
    /// </summary>
    public class CacheTime
    {
        [JsonProperty("days")]
        [Range(0, int.MaxValue, ErrorMessage = "days cannot be negative")]
        public int Days { get; set; }


        [JsonProperty("hours")]
        [Range(0, int.MaxValue, ErrorMessage = "hours cannot be negative")]
        public int Hours { get; set; }


        [JsonProperty("minutes")]
        [Range(0, int.MaxValue, ErrorMessage = "minutes cannot be negative")]
        public int Minutes { get; set; }


        [JsonProperty("seconds")]
        [Range(0, int.MaxValue, ErrorMessage = "seconds cannot be negative")]
        public int Seconds { get; set; }


        /// <summary>
        /// Converts the <see cref="CacheTime"/> instance into a <see cref="TimeSpan"/> instance.
        /// </summary>
        /// <returns>The constructed TimeSpan or <see cref="TimeSpan.Zero"/> if a valid TimeSpan cannot be created.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Specific implementation handled as documented")]
        public TimeSpan AsTimeSpan()
        {
            if (Days < 0 || Hours < 0 || Minutes < 0 || Seconds < 0)
            {
                return TimeSpan.Zero;
            }

            try
            {
                return new TimeSpan(Days, Hours, Minutes, Seconds);
            }
            catch (ArgumentOutOfRangeException)
            {
                return TimeSpan.Zero;
            }
        }
    }
}
