using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Routing;
using System.Web.SessionState;
using Swartz.Environment.Configuration;

namespace Swartz.Mvc.Routes
{
    public class RoutePublisher : IRoutePublisher
    {
        private readonly RouteCollection _routeCollection;
        private readonly ShellSettings _shellSettings;
        private readonly IWorkContextAccessor _workContextAccessor;

        public RoutePublisher(RouteCollection routeCollection, ShellSettings shellSettings,
            IWorkContextAccessor workContextAccessor)
        {
            _routeCollection = routeCollection;
            _shellSettings = shellSettings;
            _workContextAccessor = workContextAccessor;
        }


        public void Publish(IEnumerable<RouteDescriptor> routes, Func<IDictionary<string, object>, Task> pipeline = null)
        {
            var routesArray = routes
                .OrderByDescending(r => r.Priority)
                .ToArray();

            // this is not called often, but is intended to surface problems before
            // the actual collection is modified
            var preloading = new RouteCollection();
            foreach (var routeDescriptor in routesArray)
            {
                // extract the WebApi route implementation
                var httpRouteDescriptor = routeDescriptor as HttpRouteDescriptor;
                if (httpRouteDescriptor != null)
                {
                    var httpRouteCollection = new RouteCollection();
                    httpRouteCollection.MapHttpRoute(httpRouteDescriptor.Name, httpRouteDescriptor.RouteTemplate,
                        httpRouteDescriptor.Defaults, httpRouteDescriptor.Constraints);
                    routeDescriptor.Route = httpRouteCollection.First();
                }

                preloading.Add(routeDescriptor.Name, routeDescriptor.Route);
            }

            using (_routeCollection.GetWriteLock())
            {
                foreach (var routeDescriptor in routesArray)
                {
                    var shellRoute = new ShellRoute(routeDescriptor.Route, _shellSettings, _workContextAccessor,
                        pipeline)
                    {
                        IsHttpRoute = routeDescriptor is HttpRouteDescriptor,
                        SessionState = SessionStateBehavior.Default
                    };

                    _routeCollection.Add(shellRoute);
                }
            }
        }
    }
}