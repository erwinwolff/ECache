# ECache

## Example implementation

```IServiceCollection services = new ServiceCollection();
services.AddElasticSearchCache(new BlueTiger.ElasticCache.Config.ElasticCacheConfigParameters {
    CacheUrl = "http://localhost:9200"
});

var sp = services.BuildServiceProvider();

sp.UseElasticSearchCache();

await ECache.SetEntryAsync("test-entry", new { cache_entry = "cache-entry" }, TimeSpan.FromSeconds(8), true /* delay */);

bool hasEntry = await ECache.HasEntryAsync("test-entry"); // true
bool hasEntry2 = await ECache.HasEntryAsync("test-entry2"); // false

var entry = await ECache.GetEntryAsync<dynamic>("test-entry"); // non-null

Thread.Sleep(6000);

var entry2 = await ECache.GetEntryAsync<dynamic>("test-entry"); // non-null
var entry3 = await ECache.GetEntryAsync<dynamic>("test-entry2"); // null

Thread.Sleep(2000);

var entry4 = await ECache.GetEntryAsync<dynamic>("test-entry"); // expired, so null
```
