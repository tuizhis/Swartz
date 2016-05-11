using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using Swartz.Caching;
using Swartz.Environment.Configuration;
using Swartz.FileSystems.AppData;
using Swartz.OutputCache.Configuration.RouteConfig;
using Swartz.OutputCache.Models;

namespace Swartz.OutputCache.Services
{
    public class CacheService : ICacheService
    {
        private const string RouteConfigsCacheKey = "OutputCache_RouteConfigs";

        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly ITagCache _tagCache;
        private readonly IOutputCacheStorageProvider _cacheStorageProvider;
        private readonly IAppDataFolder _appDataFolder;
        private readonly ShellSettings _settings;

        public CacheService(ICacheManager cacheManager, ISignals signals, ITagCache tagCache, IOutputCacheStorageProvider cacheStorageProvider, IAppDataFolder appDataFolder, ShellSettings settings)
        {
            _cacheManager = cacheManager;
            _signals = signals;
            _tagCache = tagCache;
            _cacheStorageProvider = cacheStorageProvider;
            _appDataFolder = appDataFolder;
            _settings = settings;
        }

        public IEnumerable<CacheRouteConfig> GetRouteConfigs()
        {
            throw new NotImplementedException();
        }

        public void SaveRouteConfigs(IEnumerable<CacheRouteConfig> routeConfigs)
        {
            if (routeConfigs == null || !routeConfigs.Any())
            {
                throw new ArgumentNullException(nameof(routeConfigs));
            }

            var filePath = Path.Combine(Path.Combine("Sites", _settings.Name), "RouteConfigs");

            foreach (var file in _appDataFolder.ListFiles(filePath))
            {

            }

        }

        public string GetRouteDescriptorKey(HttpContextBase httpContext, RouteBase routeBase)
        {
            var route = routeBase as Route;
            var dataTokens = new RouteValueDictionary();

            if (route != null)
            {
                dataTokens = route.DataTokens;
            }
            else
            {
                var routeData = routeBase.GetRouteData(httpContext);

                if (routeData != null)
                {
                    dataTokens = routeData.DataTokens;
                }
            }

            var keyBuilder = new StringBuilder();

            if (route != null)
            {
                keyBuilder.AppendFormat("url={0};", route.Url);
            }

            // the data tokens are used in case the same url is used by several features, like *{path} (Rewrite Rules and Home Page Provider)
            if (dataTokens != null)
            {
                foreach (var key in dataTokens.Keys)
                {
                    keyBuilder.AppendFormat("{0}={1};", key, dataTokens[key]);
                }
            }

            return keyBuilder.ToString().ToLowerInvariant();
        }

        public void RemoveByTag(string tag)
        {
            foreach (var key in _tagCache.GetTaggedItems(tag))
            {
                _cacheStorageProvider.Remove(key);
            }

            // we no longer need the tag entry as the items have been removed
            _tagCache.RemoveTag(tag);
        }
    }
}