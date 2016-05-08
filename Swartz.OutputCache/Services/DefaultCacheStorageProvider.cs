using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using Swartz.Environment.Configuration;
using Swartz.OutputCache.Models;

namespace Swartz.OutputCache.Services
{
    public class DefaultCacheStorageProvider : IOutputCacheStorageProvider
    {
        private readonly string _tenantName;
        private readonly WorkContext _workContext;

        public DefaultCacheStorageProvider(IWorkContextAccessor workContextAccessor, ShellSettings settings)
        {
            _tenantName = settings.Name;
            _workContext = workContextAccessor.GetContext();
        }

        public void Set(string key, CacheItem cacheItem)
        {
            _workContext.HttpContext.Cache.Remove(key);
            _workContext.HttpContext.Cache.Add(key, cacheItem, null, cacheItem.StoredUntilUtc, Cache.NoSlidingExpiration,
                CacheItemPriority.Normal, null);
        }

        public void Remove(string key)
        {
            _workContext.HttpContext?.Cache.Remove(key);
        }

        public void RemoveAll()
        {
            var items = GetCacheItems(0, 100).ToList();
            while (items.Any())
            {
                foreach (var item in items)
                {
                    Remove(item.CacheKey);
                }
                items = GetCacheItems(0, 100).ToList();
            }
        }

        public CacheItem GetCacheItem(string key)
        {
            return _workContext.HttpContext.Cache.Get(key) as CacheItem;
        }

        public IEnumerable<CacheItem> GetCacheItems(int skip, int count)
        {
            // the ASP.NET cache can also contain other types of items
            return
                _workContext.HttpContext.Cache.AsParallel()
                    .Cast<DictionaryEntry>()
                    .Select(x => x.Value)
                    .OfType<CacheItem>()
                    .Where(x => x.Tenant.Equals(_tenantName, StringComparison.OrdinalIgnoreCase))
                    .Skip(skip)
                    .Take(count);
        }

        public int GetCacheItemsCount()
        {
            return
                _workContext.HttpContext.Cache.AsParallel()
                    .Cast<DictionaryEntry>()
                    .Select(x => x.Value)
                    .OfType<CacheItem>()
                    .Count(x => x.Tenant.Equals(_tenantName, StringComparison.OrdinalIgnoreCase));
        }
    }
}