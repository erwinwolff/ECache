using BlueTiger.ElasticCache.Config;
using BlueTiger.ElasticCache.Static;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlueTiger.ElasticCache.Distributed
{
    public class EDistributedCache : IDistributedCache
    {
        private Encoding _encoding = System.Text.UTF8Encoding.UTF8;

        public EDistributedCache(ElasticCacheConfigParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("Please configure ECache with AddElasticSearchCache");
        }

        public byte[] Get(string key)
        {
            return GetAsync(key).Result;
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            return _encoding.GetBytes(await ECache.GetEntryAsync<string>(key));
        }

        public void Refresh(string key)
        {
            RefreshAsync(key).Wait();
        }

        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            var value = await ECache.GetEntryAsync<byte[]>(key);

            await ECache.ClearEntryAsync(key);

            await ECache.SetEntryAsync(key, value, ECache.ElasticCacheConfigParameters.DefaultCachingPeriod);
        }

        public void Remove(string key)
        {
            RemoveAsync(key).Wait();
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            await ECache.ClearEntryAsync(key);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            SetAsync(key, value, options).Wait();
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (options.SlidingExpiration != null)
                throw new ArgumentException("Sliding Expiration not supported with BlueTiger.ElasticCache");

            await ECache.SetEntryAsync(key, value, ECache.ElasticCacheConfigParameters.DefaultCachingPeriod);
        }
    }
}