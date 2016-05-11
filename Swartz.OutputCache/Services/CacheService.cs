namespace Swartz.OutputCache.Services
{
    public class CacheService : ICacheService
    {
        private readonly IOutputCacheStorageProvider _cacheStorageProvider;
        private readonly ITagCache _tagCache;

        public CacheService(ITagCache tagCache, IOutputCacheStorageProvider cacheStorageProvider)
        {
            _tagCache = tagCache;
            _cacheStorageProvider = cacheStorageProvider;
        }

        public void RemoveByTag(string tag)
        {
            foreach (var key in _tagCache.GetTaggedItems(tag))
            {
                _cacheStorageProvider.Remove(key);
            }

            // we no longer need the tag entry as the items have been removed
            _tagCache.RemoveTag(tag);
        }
    }
}