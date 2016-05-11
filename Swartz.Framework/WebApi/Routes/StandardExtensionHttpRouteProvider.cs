using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Swartz.Environment.ShellBuilders.Models;
using Swartz.Mvc.Routes;

namespace Swartz.WebApi.Routes
{
    public class StandardExtensionHttpRouteProvider : IHttpRouteProvider
    {
        private readonly ShellBlueprint _blueprint;

        public StandardExtensionHttpRouteProvider(ShellBlueprint blueprint)
        {
            _blueprint = blueprint;
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            var displayPathsPerArea = _blueprint.HttpControllers.GroupBy(x => x.AreaName);

            return from item in displayPathsPerArea
                select item.Key
                into areaName
                let displayPath = "api/" + areaName + "/{controller}/{id}"
                select new HttpRouteDescriptor
                {
                    Priority = -10,
                    RouteTemplate = displayPath,
                    Defaults = new {area = areaName, controller = "api", id = RouteParameter.Optional}
                };
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
            {
                routes.Add(routeDescriptor);
            }
        }
    }
}