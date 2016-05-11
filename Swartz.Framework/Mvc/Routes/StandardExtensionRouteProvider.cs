using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Swartz.Environment.ShellBuilders.Models;

namespace Swartz.Mvc.Routes
{
    public class StandardExtensionRouteProvider : IRouteProvider
    {
        private readonly ShellBlueprint _blueprint;

        public StandardExtensionRouteProvider(ShellBlueprint blueprint)
        {
            _blueprint = blueprint;
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            var displayPathsPerArea = _blueprint.Controllers.GroupBy(x => x.AreaName);

            return from item in displayPathsPerArea
                select item.Key
                into areaName
                let displayPath =
                    string.IsNullOrWhiteSpace(areaName)
                        ? "{controller}/{action}/{id}"
                        : areaName + "/{controller}/{action}/{id}"
                select new RouteDescriptor
                {
                    Priority = string.IsNullOrWhiteSpace(areaName) ? -20 : -10,
                    SessionState = SessionStateBehavior.Default,
                    Route = new Route(displayPath, new RouteValueDictionary
                    {
                        {"area", areaName},
                        {"controller", "home"},
                        {"action", "index"},
                        {"id", UrlParameter.Optional}
                    }, new RouteValueDictionary(), new RouteValueDictionary
                    {
                        {"area", areaName}
                    }, new MvcRouteHandler())
                };
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }
    }
}