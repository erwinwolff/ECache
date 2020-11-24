using BlueTiger.ElasticCache.IoC;
using BlueTiger.ElasticCache.Static;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlueTigerElasticCache
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddElasticSearchCache(new BlueTiger.ElasticCache.Config.ElasticCacheConfigParameters {
                CacheUrl = "http://localhost:9200"
            });

            var sp = services.BuildServiceProvider();

            sp.UseElasticSearchCache();

            await ECache.SetEntryAsync("test-entry", new { cache_entry = "cache-entry" }, TimeSpan.FromSeconds(8), true /* delay */);

            var entry = await ECache.GetEntryAsync<dynamic>("test-entry");

            Thread.Sleep(6000);

            var entry2 = await ECache.GetEntryAsync<dynamic>("test-entry");
            var entry3 = await ECache.GetEntryAsync<dynamic>("test-entry2");


            Thread.Sleep(2000);

            var entry4 = await ECache.GetEntryAsync<dynamic>("test-entry");
        }
    }
}