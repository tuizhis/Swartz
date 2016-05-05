using System;

namespace Swartz.Caching
{
    public interface ICache<TKey, TResult>
    {
        TResult Get(TKey key, Func<AcquireContext<TKey>, TResult> acquire);
    }
}