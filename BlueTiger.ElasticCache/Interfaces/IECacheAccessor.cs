using System;
using System.Threading.Tasks;

namespace BlueTiger.ElasticCache.Interfaces
{
    public interface IECacheAccessor
    {
        Task<T> GetEntryAsync<T>(string identifier);

        Task SetEntryAsync<T>(string identifier, T entry, TimeSpan cacheLength, bool delayAfterInsert = false);

        Task ClearEntryAsync(string identifier);

        Task<bool> HasEntryAsync(string identifier);
    }
}