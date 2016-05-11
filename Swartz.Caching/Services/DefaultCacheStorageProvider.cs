using System;
using System.Globalization;
using System.Runtime.Caching;
using Swartz.Services;

namespace Swartz.Caching.Services
{
    /// <summary>
    ///     The technique of signaling tenant-specific cache entries to be invalidated comes from:
    ///     http://stackoverflow.com/a/22388943/220230
    ///     Singleton so signals can be stored for the shell lifetime.
    /// </summary>
    public class DefaultCacheStorageProvider : ICacheStorageProvider, ISingletonDependency
    {
        // MemoryCache is optimal with one instance, see: http://stackoverflow.com/questions/8463962/using-multiple-instances-of-memorycache/13425322#13425322
        private readonly MemoryCache _cache = MemoryCache.Default;

        private readonly IClock _clock;

        public DefaultCacheStorageProvider(IClock clock)
        {
            _clock = clock;
        }

        public object Get<T>(string key)
        {
            return _cache.Get(key);
        }

        public void Put<T>(string key, T value)
        {
            _cache.Set(key, value, GetCacheItemPolicy(ObjectCache.InfiniteAbsoluteExpiration));
        }

        public void Put<T>(string key, T value, TimeSpan validFor)
        {
            _cache.Set(key, value, GetCacheItemPolicy(new DateTimeOffset(_clock.UtcNow.Add(validFor))));
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void Clear()
        {
            Signaled?.Invoke(null, EventArgs.Empty);
        }

        private event EventHandler Signaled;

        private CacheItemPolicy GetCacheItemPolicy(DateTimeOffset absoluteExpiration)
        {
            var cacheItemPolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = absoluteExpiration,
                SlidingExpiration = ObjectCache.NoSlidingExpiration
            };

            cacheItemPolicy.ChangeMonitors.Add(new TenantCacheClearMonitor(this));

            return cacheItemPolicy;
        }

        public class TenantCacheClearMonitor : ChangeMonitor
        {
            private readonly DefaultCacheStorageProvider _storageProvider;

            public TenantCacheClearMonitor(DefaultCacheStorageProvider storageProvider)
            {
                _storageProvider = storageProvider;
                _storageProvider.Signaled += OnSignalRaised;
                InitializationComplete();
            }

            public override string UniqueId { get; } = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);

            protected override void Dispose(bool disposing)
            {
                Dispose();
                _storageProvider.Signaled -= OnSignalRaised;
            }

            private void OnSignalRaised(object sender, EventArgs e)
            {
                // Cache objects are obligated to remove entry upon change notification.
                OnChanged(null);
            }
        }
    }
}