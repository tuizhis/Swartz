using System.Collections.Generic;
using Swartz.Mvc.Routes;

namespace Swartz.WebApi.Routes
{
    public interface IHttpRouteProvider : IDependency
    {
        IEnumerable<RouteDescriptor> GetRoutes();

        void GetRoutes(ICollection<RouteDescriptor> routes);
    }
}