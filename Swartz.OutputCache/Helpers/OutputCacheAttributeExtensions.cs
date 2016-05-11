using System.Linq;
using System.Web.UI;
using Swartz.OutputCache.Filters;

namespace Swartz.OutputCache.Helpers
{
    public static class OutputCacheAttributeExtensions
    {
        /// <summary>
        ///     Returns true if the Location of the specified output cache attribute matches any of the specified list of
        ///     locations.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="locations"></param>
        /// <returns></returns>
        public static bool LocationIsIn(this SwartzOutputCacheAttribute attribute,
            params OutputCacheLocation[] locations)
        {
            return locations.Contains(attribute.Location);
        }
    }
}