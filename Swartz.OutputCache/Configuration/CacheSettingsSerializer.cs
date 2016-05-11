using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Swartz.OutputCache.Configuration
{
    public class CacheSettingsSerializer
    {
        public const char Separator = ':';
        public const string EmptyValue = "null";
        public const string ValueSeparator = ",";

        public static CacheSettings ParseSettings(string text)
        {
            var cacheSettings = new CacheSettings();
            if (string.IsNullOrWhiteSpace(text))
            {
                return cacheSettings;
            }

            var settings = new StringReader(text);
            string setting;
            while ((setting = settings.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(setting)) continue;
                var separatorIndex = setting.IndexOf(Separator);
                if (separatorIndex == -1) continue;

                var key = setting.Substring(0, separatorIndex).Trim();
                var value = setting.Substring(separatorIndex + 1).Trim();

                if (!value.Equals(EmptyValue, StringComparison.OrdinalIgnoreCase))
                {
                    switch (key)
                    {
                        case "DefaultCacheDuration":
                            cacheSettings.DefaultCacheDuration = Convert.ToInt32(value);
                            break;
                        case "DefaultCacheGraceTime":
                            cacheSettings.DefaultCacheGraceTime = Convert.ToInt32(value);
                            break;
                        case "DefaultMaxAge":
                            cacheSettings.DefaultMaxAge = Convert.ToInt32(value);
                            break;
                        case "VaryByQueryStringParameters":
                            cacheSettings.VaryByQueryStringParameters =
                                value.Split(new[] {ValueSeparator}, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => s.Trim())
                                    .ToArray();
                            break;
                        case "VaryByRequestHeaders":
                            cacheSettings.VaryByRequestHeaders =
                                new HashSet<string>(
                                    value.Split(new[] {ValueSeparator}, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => s.Trim())
                                        .ToArray());
                            break;
                        case "IgnoredUrls":
                            cacheSettings.IgnoredUrls =
                                value.Split(new[] {ValueSeparator}, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => s.Trim())
                                    .ToArray();
                            break;
                        case "IgnoreNoCache":
                            cacheSettings.IgnoreNoCache = Convert.ToBoolean(value);
                            break;
                        case "CacheAuthenticatedRequests":
                            cacheSettings.CacheAuthenticatedRequests = Convert.ToBoolean(value);
                            break;
                        case "VaryByAuthenticationState":
                            cacheSettings.VaryByAuthenticationState = Convert.ToBoolean(value);
                            break;
                        case "DebugMode":
                            cacheSettings.DebugMode = Convert.ToBoolean(value);
                            break;
                    }
                }
            }

            return cacheSettings;
        }

        public static string ComposeSettings(CacheSettings settings)
        {
            if (settings == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var key in settings.Keys)
            {
                sb.AppendLine(key + ": " + (settings[key] ?? EmptyValue));
            }

            return sb.ToString();
        }
    }
}