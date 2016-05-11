using System.Collections.Generic;

namespace Swartz.Mvc.Routes
{
    public interface IRouteProvider : IDependency
    {
        IEnumerable<RouteDescriptor> GetRoutes();

        void GetRoutes(ICollection<RouteDescriptor> routes);
    }
}