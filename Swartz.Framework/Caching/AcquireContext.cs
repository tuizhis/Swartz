﻿using System;

namespace Swartz.Caching
{
    public interface IAcquireContext
    {
        Action<IVolatileToken> Monitor { get; }
    }

    public class AcquireContext<TKey> : IAcquireContext
    {
        public AcquireContext(TKey key, Action<IVolatileToken> monitor)
        {
            Key = key;
            Monitor = monitor;
        }

        public TKey Key { get; private set; }
        public Action<IVolatileToken> Monitor { get; }
    }

    /// <summary>
    ///     Simple implementation of "IAcquireContext" given a lamdba
    /// </summary>
    public class SimpleAcquireContext : IAcquireContext
    {
        public SimpleAcquireContext(Action<IVolatileToken> monitor)
        {
            Monitor = monitor;
        }

        public Action<IVolatileToken> Monitor { get; }
    }
}