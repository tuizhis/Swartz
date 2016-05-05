using System;
using System.Collections;
using Autofac.Core;

namespace Swartz.Environment
{
    internal class CollectionOrderModule : IModule
    {
        public void Configure(IComponentRegistry componentRegistry)
        {
            componentRegistry.Registered += (s, e) =>
            {
                // only bother watching enumerable resolves
                var limitType = e.ComponentRegistration.Activator.LimitType;
                if (typeof(IEnumerable).IsAssignableFrom(limitType))
                {
                    e.ComponentRegistration.Activated += (s2, e2) =>
                    {
                        var array = e2.Instance as Array;
                        if (array != null)
                        {
                            Array.Reverse(array);
                        }
                    };
                }
            };
        }
    }
}