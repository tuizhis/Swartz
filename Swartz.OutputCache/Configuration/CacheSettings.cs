using System;
using System.Collections.Generic;
using System.Linq;

namespace Swartz.OutputCache.Configuration
{
    public class CacheSettings
    {
        private const string Separator = ",";
        private readonly IDictionary<string, string> _values;
        public const string CacheKey = "Swartz_OutputCache_CacheSettings";

        public CacheSettings()
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

        public int DefaultCacheDuration
        {
            get
            {
                if (this["DefaultCacheDuration"] == null)
                {
                    return 300;
                }

                return Convert.ToInt32(this["DefaultCacheDuration"]);
            }
            set { this["DefaultCacheDuration"] = Convert.ToString(value); }
        }

        public int DefaultCacheGraceTime
        {
            get
            {
                if (this["DefaultCacheGraceTime"] == null)
                {
                    return 60;
                }

                return Convert.ToInt32(this["DefaultCacheGraceTime"]);
            }
            set { this["DefaultCacheGraceTime"] = Convert.ToString(value); }
        }

        public int DefaultMaxAge
        {
            get
            {
                if (this["DefaultMaxAge"] == null)
                {
                    return 0;
                }
                return Convert.ToInt32(this["DefaultMaxAge"]);
            }
            set { this["DefaultMaxAge"] = Convert.ToString(value); }
        }

        public IEnumerable<string> VaryByQueryStringParameters
        {
            get
            {
                return this["VaryByQueryStringParameters"]?.Split(new[] {Separator},
                    StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
            }
            set
            {
                if (value == null || !value.Any())
                {
                    this["VaryByQueryStringParameters"] = null;
                    return;
                }
                this["VaryByQueryStringParameters"] = string.Join(Separator, value);
            }
        }

        public ISet<string> VaryByRequestHeaders
        {
            get
            {
                if (this["VaryByRequestHeaders"] == null)
                {
                    return null;
                }
                return
                    new HashSet<string>(
                        this["VaryByRequestHeaders"].Split(new[] {Separator}, StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .ToArray());
            }
            set
            {
                if (value == null || !value.Any())
                {
                    this["VaryByRequestHeaders"] = null;
                    return;
                }
                this["VaryByRequestHeaders"] = string.Join(Separator, value);
            }
        }

        public IEnumerable<string> IgnoredUrls
        {
            get
            {
                return
                    this["IgnoredUrls"]?.Split(new[] {Separator}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .ToArray();
            }
            set
            {
                if (value == null || !value.Any())
                {
                    this["IgnoredUrls"] = null;
                    return;
                }
                this["IgnoredUrls"] = string.Join(Separator, value);
            }
        }

        public bool IgnoreNoCache
        {
            get
            {
                if (this["IgnoreNoCache"] == null)
                {
                    return false;
                }

                return Convert.ToBoolean(this["IgnoreNoCache"]);
            }
            set { this["IgnoreNoCache"] = Convert.ToString(value); }
        }

        public bool CacheAuthenticatedRequests
        {
            get
            {
                if (this["CacheAuthenticatedRequests"] == null)
                {
                    return false;
                }
                return Convert.ToBoolean(this["CacheAuthenticatedRequests"]);
            }
            set { this["CacheAuthenticatedRequests"] = Convert.ToString(value); }
        }

        public bool VaryByAuthenticationState
        {
            get
            {
                if (this["VaryByAuthenticationState"] == null)
                {
                    return false;
                }
                return Convert.ToBoolean(this["VaryByAuthenticationState"]);
            }
            set { this["VaryByAuthenticationState"] = Convert.ToString(value); }
        }

        public bool DebugMode
        {
            get
            {
                if (this["DebugMode"] == null)
                {
                    return false;
                }
                return Convert.ToBoolean(this["DebugMode"]);
            }
            set { this["DebugMode"] = Convert.ToString(value); }
        }
    }
}