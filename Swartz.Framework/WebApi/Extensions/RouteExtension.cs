using System.Web.Http.Routing;
using System.Web.Mvc;
using System.Web.Routing;

namespace Swartz.WebApi.Extensions
{
    public static class RouteExtension
    {
        public static string GetAreaName(this IHttpRoute route)
        {
            var routeWithArea = route as IRouteWithArea;
            if (routeWithArea != null)
            {
                return routeWithArea.Area;
            }

            var castRoute = route as Route;
            return castRoute?.DataTokens?["area"] as string;
        }

        public static string GetAreaName(this IHttpRouteData routeData)
        {
            object area;
            if (routeData.Route.Defaults.TryGetValue("area", out area))
            {
                return area as string;
            }

            return GetAreaName(routeData.Route);
        }
    }
}