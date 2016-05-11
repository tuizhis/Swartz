using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Swartz.OutputCache.Configuration.RouteConfig;

namespace Swartz.OutputCache
{
    public interface ICacheService : IDependency
    {
        /// <summary>
        ///     Returns all defined configurations for specific routes.
        /// </summary>
        /// <returns></returns>
        IEnumerable<CacheRouteConfig> GetRouteConfigs();

        /// <summary>
        ///     Saves a set of <see cref="CacheRouteConfig" /> to the database.
        /// </summary>
        /// <param name="routeConfigs"></param>
        void SaveRouteConfigs(IEnumerable<CacheRouteConfig> routeConfigs);

        /// <summary>
        ///     Returns the key representing a specific route in the DB.
        /// </summary>
        string GetRouteDescriptorKey(HttpContextBase httpContext, RouteBase route);

        /// <summary>
        ///     Removes all cache entries associated with a specific tag.
        /// </summary>
        /// <param name="tag">The tag value.</param>
        void RemoveByTag(string tag);
    }
}