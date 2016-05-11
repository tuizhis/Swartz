using System;
using System.Web.UI;

namespace Swartz.OutputCache.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SwartzOutputCacheAttribute : Attribute
    {
        public int Duration { get; set; }

        public int GraceTime { get; set; }

        public bool NoStore { get; set; }

        public OutputCacheLocation Location { get; set; }
    }
}