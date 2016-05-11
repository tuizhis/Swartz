using System;
using Swartz.Environment.Configuration;

namespace Swartz.Caching.Services
{
    /// <summary>
    ///     Provides a per tenant <see cref="ICacheService" /> implementation.
    /// </summary>
    public class DefaultCacheService : ICacheService
    {
        private readonly ICacheStorageProvider _cacheStorageProvider;
        private readonly string _prefix;

        public DefaultCacheService(ShellSettings settings, ICacheStorageProvider cacheStorageProvider)
        {
            _cacheStorageProvider = cacheStorageProvider;
            _prefix = settings.Name;
        }

        public object GetObject<T>(string key)
        {
            return _cacheStorageProvider.Get<T>(BuildFullKey(key));
        }

        public void Put<T>(string key, T value)
        {
            _cacheStorageProvider.Put(BuildFullKey(key), value);
        }

        public void Put<T>(string key, T value, TimeSpan validFor)
        {
            _cacheStorageProvider.Put(BuildFullKey(key), value, validFor);
        }

        public void Remove(string key)
        {
            _cacheStorageProvider.Remove(BuildFullKey(key));
        }

        public void Clear()
        {
            _cacheStorageProvider.Clear();
        }

        private string BuildFullKey(string key)
        {
            return string.Concat(_prefix, ":", key);
        }
    }
}