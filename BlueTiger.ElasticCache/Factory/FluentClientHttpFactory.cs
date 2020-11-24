using BlueTiger.ElasticCache.Config;
using Microsoft.Extensions.Logging;
using Pathoschild.Http.Client;
using System;
using System.Net.Http;

namespace BlueTiger.ElasticCache.Factory
{
    public class FluentClientHttpFactory
    {
        private static IClient HttpClient;

        public FluentClientHttpFactory(HttpClient httpClient, ElasticCacheConfigParameters elasticCacheConfigParameters, ILogger<FluentClientHttpFactory> logger)
        {
            if (string.IsNullOrEmpty(elasticCacheConfigParameters.CacheUrl))
                throw new ArgumentNullException(nameof(elasticCacheConfigParameters.CacheUrl));

            if (HttpClient == null)
            {
                logger.LogDebug("FluentClient for ECache created");
                HttpClient = new FluentClient(new Uri(elasticCacheConfigParameters.CacheUrl), httpClient)
                    .SetOptions(ignoreHttpErrors: true).SetUserAgent(".NET Core ECache");
            }
        }

        public IClient Create()
        {
            return HttpClient;
        }
    }
}