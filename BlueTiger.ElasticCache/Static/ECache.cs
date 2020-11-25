using BlueTiger.ElasticCache.Accessor;
using BlueTiger.ElasticCache.Config;
using BlueTiger.ElasticCache.Dto;
using BlueTiger.ElasticCache.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pathoschild.Http.Client;
using Polly;
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BlueTiger.ElasticCache.Static
{
    public static class ECache
    {
        static ECache()
        {
        }

        internal static IClient HttpClient { get; set; }
        internal static ElasticCacheConfigParameters ElasticCacheConfigParameters { get; set; }
        internal static ILogger<ECacheAccessor> Logger { get; set; }

        public static async Task<bool> HasEntryAsync(string identifier)
        {
            SearchResultDto entryInElastic = null;

            await HttpClientToCachePolicy().ExecuteAsync(async () =>
            {
                entryInElastic = await HttpClient
                     .GetAsync($"{ElasticCacheConfigParameters.CacheUrl}/{ElasticCacheConfigParameters.IndexName}/_search?q={identifier}")
                     .As<SearchResultDto>();
            });

            if (entryInElastic.hits != null &&
                entryInElastic.hits.total != null &&
                entryInElastic.hits.total.value > 0)
            {
                foreach (var item in entryInElastic.hits.hits)
                {
                    long epochDate = (long)DateTime.UtcNow.Subtract(
                        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                        ).TotalMilliseconds;

                    if (item._source.Identifier == identifier &&
                        item._source.ValidUntil < epochDate)
                    {
                        Logger.LogDebug("Deleting expired entry '{0}'", item._id);

                        await ClearEntryAsync(identifier);
                        return false;
                    }

                    if (item._source.Identifier == identifier)
                        return true;
                }
            }

            return false;
        }

        public static async Task<T> GetEntryAsync<T>(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentNullException(nameof(identifier));

            Logger.LogDebug("Retrieving entry '{0}'", identifier);

            T result = default(T);

            SearchResultDto entryInElastic = null;

            await HttpClientToCachePolicy().ExecuteAsync(async () =>
            {
                entryInElastic = await HttpClient
                   .GetAsync($"{ElasticCacheConfigParameters.CacheUrl}/{ElasticCacheConfigParameters.IndexName}/_search?q={identifier}")
                   .As<SearchResultDto>();
            });

            if (entryInElastic.hits != null &&
                entryInElastic.hits.total != null &&
                entryInElastic.hits.total.value > 0)
            {
                foreach (var item in entryInElastic.hits.hits)
                {
                    long epochDate = (long)DateTime.UtcNow.Subtract(
                        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                        ).TotalMilliseconds;

                    if (item._source.Identifier == identifier &&
                        item._source.ValidUntil < epochDate)
                    {
                        Logger.LogDebug("Deleting expired entry '{0}'", item._id);

                        await ClearEntryAsync(identifier);
                        continue;
                    }

                    if (item._source.Identifier == identifier)
                    {
                        Logger.LogDebug("Found entry '{0}' took {1}ms", identifier, entryInElastic.took);

                        result = JsonConvert.DeserializeObject<T>(item._source.JsonContents);
                    }
                }
            }

            return result;
        }

        public static async Task SetEntryAsync<T>(string identifier, T entry, TimeSpan cacheLength, bool delayAfterInsert = false)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentNullException(nameof(identifier));

            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            if (cacheLength == default(TimeSpan))
                throw new ArgumentNullException(nameof(cacheLength));

            Logger.LogDebug("Setting entry '{0}'", identifier);

            SearchResultDto entryInElastic = null;

            await HttpClientToCachePolicy().ExecuteAsync(async () =>
            {
                entryInElastic = await HttpClient
                   .GetAsync($"{ElasticCacheConfigParameters.CacheUrl}/{ElasticCacheConfigParameters.IndexName}/_search?q={identifier}")
                   .As<SearchResultDto>();
            });

            if (entryInElastic.hits != null &&
                entryInElastic.hits.total != null &&
                entryInElastic.hits.total.value > 0)
            {
                foreach (var item in entryInElastic.hits.hits)
                {
                    if (item._source.Identifier == identifier)
                    {
                        Logger.LogDebug("Deleting exsting entry '{0}'", item._id);

                        await HttpClientToCachePolicy().ExecuteAsync(async () =>
                        {
                            await HttpClient
                            .DeleteAsync($"{ElasticCacheConfigParameters.CacheUrl}/{ElasticCacheConfigParameters.IndexName}/_doc/{item._id}")
                            .AsString();
                        });
                    }
                }
            }

            IResponse response = null;

            await HttpClientToCachePolicy().ExecuteAsync(async () =>
            {
                response = await HttpClient
                   .PostAsync($"{ElasticCacheConfigParameters.CacheUrl}/{ElasticCacheConfigParameters.IndexName}/_doc/",
                   new CacheEntryDto
                   {
                       Identifier = identifier,
                       JsonContents = JsonConvert.SerializeObject(entry),
                       ValidUntil = (long)(DateTime.UtcNow + cacheLength).Subtract(
                           new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                           ).TotalMilliseconds
                   });
            });

            if (delayAfterInsert)
                await Task.Delay(ElasticCacheConfigParameters.DelayAfterInsert);

            if (!response.IsSuccessStatusCode &&
                response.Status == System.Net.HttpStatusCode.NotFound &&
                ElasticCacheConfigParameters.ThrowExceptionOnNotFound)
                throw new ECacheIndexDoesNotExistException(identifier);
        }

        public static async Task ClearEntryAsync(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentNullException(nameof(identifier));

            SearchResultDto entryInElastic = null;

            await HttpClientToCachePolicy().ExecuteAsync(async () =>
            {
                entryInElastic = await HttpClient
                   .GetAsync($"{ElasticCacheConfigParameters.CacheUrl}/{ElasticCacheConfigParameters.IndexName}/_search?q={identifier}")
                   .As<SearchResultDto>();
            });

            if (entryInElastic.hits != null &&
                entryInElastic.hits.total != null &&
                entryInElastic.hits.total.value > 0)
            {
                foreach (var item in entryInElastic.hits.hits)
                {
                    if (item._source.Identifier == identifier)
                    {
                        Logger.LogDebug("Deleting exsting entry '{0}'", item._id);

                        await HttpClientToCachePolicy().ExecuteAsync(async () =>
                        {
                            await HttpClient
                                .DeleteAsync($"{ElasticCacheConfigParameters.CacheUrl}/{ElasticCacheConfigParameters.IndexName}/_doc/{item._id}")
                                .AsString();
                        });
                    }
                }
            }
        }

        internal static AsyncPolicy HttpClientToCachePolicy()
        {
            var handlePolicy = Policy.HandleInner<SocketException>()
                .Or<HttpRequestException>()
                .Or<ApiException>()
                .Or<AggregateException>()
                .WaitAndRetryAsync(ElasticCacheConfigParameters.MaxRetriesToCache, sleep => TimeSpan.FromSeconds(ElasticCacheConfigParameters.MaxTimeOutInSeconds));

            return handlePolicy;
        }
    }
}