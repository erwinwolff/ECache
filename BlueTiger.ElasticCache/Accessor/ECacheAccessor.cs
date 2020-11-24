using BlueTiger.ElasticCache.Interfaces;
using BlueTiger.ElasticCache.Static;
using System;
using System.Threading.Tasks;

namespace BlueTiger.ElasticCache.Accessor
{
    internal class ECacheAccessor : IECacheAccessor
    {
        public async Task ClearEntryAsync(string identifier)
        {
            await ECache.ClearEntryAsync(identifier);
        }

        public async Task<T> GetEntryAsync<T>(string identifier)
        {
            return await ECache.GetEntryAsync<T>(identifier);
        }

        public async Task<bool> HasEntryAsync(string identifier)
        {
            return await ECache.HasEntryAsync(identifier);
        }

        public async Task SetEntryAsync<T>(string identifier, T entry, TimeSpan cacheLength, bool delayAfterInsert = false)
        {
            await ECache.SetEntryAsync<T>(identifier, entry, cacheLength, delayAfterInsert);
        }
    }
}