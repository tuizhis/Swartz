using System;

namespace Swartz.Caching
{
    public interface ICacheHolder : ISingletonDependency
    {
        ICache<TKey, TResult> GetCache<TKey, TResult>(Type component);
    }
}