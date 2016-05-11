using System;
using System.Collections.Generic;

namespace Swartz.OutputCache.Configuration.RouteConfig
{
    public class CacheRouteConfig
    {
        private readonly IDictionary<string, string> _values;

        public CacheRouteConfig()
        {
            _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public string this[string key]
        {
            get
            {
                string retVal;
                return _values.TryGetValue(key, out retVal) ? retVal : null;
            }
            set { _values[key] = value; }
        }

        public IEnumerable<string> Keys => _values.Keys;

        public string RouteKey
        {
            get { return this["RouteKey"] ?? ""; }
            set { this["RouteKey"] = value; }
        }

        public string Url
        {
            get { return this["Url"] ?? ""; }
            set { this["Url"] = value; }
        }

        public int? Duration
        {
            get
            {
                if (this["Duration"] == null) return null;

                return Convert.ToInt32(this["Duration"]);
            }
            set { this["Duration"] = value.HasValue ? Convert.ToString(value.Value) : null; }
        }

        public int? GraceTime
        {
            get
            {
                if (this["GraceTime"] == null)
                {
                    return null;
                }

                return Convert.ToInt32(this["GraceTime"]);
            }
            set { this["GraceTime"] = value.HasValue ? Convert.ToString(value.Value) : null; }
        }

        public int? MaxAge
        {
            get
            {
                if (this["MaxAge"] == null)
                {
                    return null;
                }

                return Convert.ToInt32(this["MaxAge"]);
            }
            set { this["MaxAge"] = value.HasValue ? Convert.ToString(value.Value) : null; }
        }
    }
}