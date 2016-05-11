using System;

namespace Swartz.Caching
{
    public interface ICacheManager
    {
        TResult Get<TKey, TResult>(TKey key, Func<AcquireContext<TKey>, TResult> acquire);
        ICache<TKey, TResult> GetCache<TKey, TResult>();
    }

    public static class CacheManagerExtensions
    {
        public static TResult Get<TKey, TResult>(this ICacheManager cacheManager, TKey key, bool preventConcurrentCalls,
            Func<AcquireContext<TKey>, TResult> acquire) where TKey : class 
        {
            if (preventConcurrentCalls)
            {
                lock (key)
                {
                    return cacheManager.Get(key, acquire);
                }
            }
            return cacheManager.Get(key, acquire);
        }
    }
}