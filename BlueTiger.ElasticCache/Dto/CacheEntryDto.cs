namespace BlueTiger.ElasticCache.Dto
{
    public class CacheEntryDto
    {
        public string Identifier { get; set; }
        public string JsonContents { get; set; }
        public long ValidUntil { get; set; }
    }
}