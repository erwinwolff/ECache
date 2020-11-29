using Microsoft.Extensions.Caching.Distributed;
using System;

namespace BlueTiger.ElasticCache.Config
{
    public class ElasticCacheConfigParameters
    {
        /// <summary>
        /// The maximum amount of retries to the cache instance
        /// </summary>
        public int MaxRetriesToCache { get; set; } = 3;

        /// <summary>
        /// The maximum TimeOut to the retrieve the cache in seconds
        /// </summary>
        public int MaxTimeOutInSeconds { get; set; } = 5;

        /// <summary>
        /// Standard behaviour is to return null when not found, but an <see cref="Exceptions.ECacheEntryNotFoundException"/> can be thrown
        /// </summary>
        public bool ThrowExceptionOnNotFound { get; set; } = false;

        /// <summary>
        /// The URL to Elastic Search instance
        /// </summary>
        public string CacheUrl { get; set; } = string.Empty;

        /// <summary>
        /// The name of the used index. The default is 'ecachedotnetstore'
        /// </summary>
        public string IndexName { get; set; } = "ecachedotnetstore";

        /// <summary>
        /// The number of ElasticSearch shards
        /// </summary>
        public int NumberOfShards { get; set; } = 2;

        /// <summary>
        /// Delay after an insert operation
        /// </summary>
        public int DelayAfterInsert { get; set; } = 750;

        /// <summary>
        /// The caching period for the <see cref="IDistributedCache"/> Implementation
        /// </summary>
        public TimeSpan DefaultCachingPeriod { get; set; } = TimeSpan.FromMinutes(10);
    }
}