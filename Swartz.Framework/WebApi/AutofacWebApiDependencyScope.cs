using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using Autofac;

namespace Swartz.WebApi
{
    public class AutofacWebApiDependencyScope : IDependencyScope
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacWebApiDependencyScope(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public void Dispose()
        {
            _lifetimeScope?.Dispose();
        }

        public object GetService(Type serviceType)
        {
            return _lifetimeScope.ResolveOptional(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (!_lifetimeScope.IsRegistered(serviceType))
            {
                return Enumerable.Empty<object>();
            }

            var enumerableServiceType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            var instance = _lifetimeScope.Resolve(enumerableServiceType);
            return (IEnumerable<object>) instance;
        }
    }
}