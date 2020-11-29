using BlueTiger.ElasticCache.Accessor;
using BlueTiger.ElasticCache.Config;
using BlueTiger.ElasticCache.Distributed;
using BlueTiger.ElasticCache.Exceptions;
using BlueTiger.ElasticCache.Factory;
using BlueTiger.ElasticCache.FluentClientExtensions;
using BlueTiger.ElasticCache.Interfaces;
using BlueTiger.ElasticCache.Static;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pathoschild.Http.Client;
using System;
using System.Threading.Tasks;

namespace BlueTiger.ElasticCache.IoC
{
    public static class BlueTigerElasticSearchCacheIoC
    {
        public static IServiceCollection AddElasticSearchCache(this IServiceCollection services, ElasticCacheConfigParameters config)
        {
            services.AddSingleton(config);
            services.AddTransient<IECacheAccessor, ECacheAccessor>();
            services.AddHttpClient<FluentClientHttpFactory>();

            ECache.ElasticCacheConfigParameters = config;

            return services;
        }

        public static IServiceCollection AddElasticSearchDistributedCache(this IServiceCollection services)
        {
            services.AddSingleton<IDistributedCache, EDistributedCache>();

            return services;
        }

        public static void UseElasticSearchCache(this IServiceProvider serviceProvider)
        {
            ECache.HttpClient = serviceProvider.GetService<FluentClientHttpFactory>().Create();
            ECache.Logger = serviceProvider.GetService<ILogger<ECacheAccessor>>();

            Task.Run(async () => {
                string indexUrl = $"{ECache.ElasticCacheConfigParameters.CacheUrl}/{ECache.ElasticCacheConfigParameters.IndexName}/";

                bool indexExists = false;

                await ECache.HttpClientToCachePolicy().ExecuteAsync(async () => { 
                    indexExists = indexExists = (await ECache.HttpClient.SetOptions(ignoreHttpErrors: true).HeadAsync(indexUrl)).Status == System.Net.HttpStatusCode.OK;
                });
                
                if (!indexExists)
                {
                    ECache.Logger.LogInformation("Creating ECache index ...");
                    IResponse creationResult = null;
                    await ECache.HttpClientToCachePolicy().ExecuteAsync(async () => {
                        creationResult = await ECache.HttpClient.PutAsync(indexUrl, new
                        {
                            settings = new
                            {
                                index = new
                                {
                                    number_of_shards = ECache.ElasticCacheConfigParameters.NumberOfShards
                                }
                            },
                            mappings = new
                            {
                                properties = new
                                {
                                    Identifier = new { type = "keyword" },
                                    JsonContents = new { type = "text" },
                                    ValidUntil = new { type = "date" }
                                }
                            }
                        }).AsResponse();
                    });

                    if (!creationResult.IsSuccessStatusCode)
                        throw new ECacheIndexDoesNotExistException(ECache.ElasticCacheConfigParameters.IndexName);
                }

            }).Wait();
        }
    }
}