using System;
using System.IO;
using System.Text;

namespace Swartz.OutputCache.Configuration.RouteConfig
{
    public class RouteConfigSerializer
    {
        public const char Separator = ':';
        public const string EmptyValue = "null";

        public static CacheRouteConfig ParseSettings(string text)
        {
            var cacheRouteConfig = new CacheRouteConfig();
            if (string.IsNullOrWhiteSpace(text))
            {
                return cacheRouteConfig;
            }

            var configs = new StringReader(text);
            string config;
            while ((config = configs.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(config)) continue;
                var separatorIndex = config.IndexOf(Separator);
                if (separatorIndex == -1) continue;

                var key = config.Substring(0, separatorIndex).Trim();
                var value = config.Substring(separatorIndex + 1).Trim();

                if (!value.Equals(EmptyValue, StringComparison.OrdinalIgnoreCase))
                {
                    switch (key)
                    {
                        case "RouteKey":
                            cacheRouteConfig.RouteKey = value;
                            break;
                        case "Url":
                            cacheRouteConfig.Url = value;
                            break;
                        case "Duration":
                            cacheRouteConfig.Duration = Convert.ToInt32(value);
                            break;
                        case "GraceTime":
                            cacheRouteConfig.GraceTime = Convert.ToInt32(value);
                            break;
                        case "MaxAge":
                            cacheRouteConfig.MaxAge = Convert.ToInt32(value);
                            break;
                    }
                }
            }

            return cacheRouteConfig;
        }

        public static string ComposeSettings(CacheRouteConfig config)
        {
            if (config == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var key in config.Keys)
            {
                sb.AppendLine(key + ":" + (config[key] ?? EmptyValue));
            }

            return sb.ToString();
        }
    }
}