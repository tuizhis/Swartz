using System.Collections.Generic;
using Swartz.Caching;

namespace Swartz.Environment
{
    public static class WebHostContainerRegistry
    {
        private static readonly IList<Weak<IShim>> Shims = new List<Weak<IShim>>();
        private static IWebHostContainer _hostContainer;
        private static readonly object SyncLock = new object();

        public static void RegisterShim(IShim shim)
        {
            lock (SyncLock)
            {
                CleanupShims();

                Shims.Add(new Weak<IShim>(shim));
                shim.HostContainer = _hostContainer;
            }
        }

        public static void RegisterHostContainer(IWebHostContainer container)
        {
            lock (SyncLock)
            {
                CleanupShims();

                _hostContainer = container;
                RegisterContainerInShims();
            }
        }

        private static void RegisterContainerInShims()
        {
            foreach (var shim in Shims)
            {
                var target = shim.Target;
                if (target != null)
                {
                    target.HostContainer = _hostContainer;
                }
            }
        }

        private static void CleanupShims()
        {
            for (var i = Shims.Count - 1; i >= 0; i--)
            {
                if (Shims[i].Target == null)
                {
                    Shims.RemoveAt(i);
                }
            }
        }
    }
}