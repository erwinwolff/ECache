using Pathoschild.Http.Client;
using System.Net.Http;

namespace BlueTiger.ElasticCache.FluentClientExtensions
{
    public static class ExtensionMethods
    {
        public static IRequest HeadAsync(this IClient client, string resource)
        {
            return client.SendAsync(HttpMethod.Head, resource);
        }
    }
}