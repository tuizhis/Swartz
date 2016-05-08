using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using Swartz.Caching;
using Swartz.Environment.Configuration;
using Swartz.Logging;
using Swartz.OutputCache.Helpers;
using Swartz.OutputCache.Models;
using Swartz.Services;
using IFilterProvider = Swartz.Mvc.Filters.IFilterProvider;

namespace Swartz.OutputCache.Filters
{
    public class OutputCacheFilter : IFilterProvider, IActionFilter, IResultFilter, IDisposable
    {
        private static string _refreshKey = "__r";
        private static long _epoch = new DateTime(2014, DateTimeKind.Utc).Ticks;

        private readonly ICacheManager _cacheManager;
        private readonly IOutputCacheStorageProvider _cacheStorageProvider;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IClock _clock;
        private readonly ShellSettings _settings;

        public ILogger Logger { get; set; }


        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            throw new NotImplementedException();
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            throw new NotImplementedException();
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            throw new NotImplementedException();
        }

        private void ReleaseCacheKeyLock()
        {
        }

        protected virtual bool RequestIsCacheable(ActionExecutingContext filterContext)
        {
            // Respect OutputCacheAttribute if applied.
            var actionAttributes = filterContext.ActionDescriptor.GetCustomAttributes(typeof(OutputCacheAttribute), true);
            var controllerAttributes =
                filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(OutputCacheAttribute),
                    true);
            var outputCacheAttribute =
                actionAttributes.Concat(controllerAttributes).Cast<OutputCacheAttribute>().FirstOrDefault();

            if (outputCacheAttribute != null)
            {
                if (outputCacheAttribute.Duration <= 0 || outputCacheAttribute.NoStore ||
                    outputCacheAttribute.LocationIsIn(OutputCacheLocation.Downstream, OutputCacheLocation.Client,
                        OutputCacheLocation.None))
                {
                    return false;
                }
            }

            // Don't cache POST requests.
            if (filterContext.HttpContext.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Ignore requests with the refresh key on the query string.
            foreach (var key in filterContext.RequestContext.HttpContext.Request.QueryString.AllKeys)
            {
                if (string.Equals(_refreshKey, key, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        protected virtual bool IsIgnoredUrl(string url, IEnumerable<string> ignoredUrls)
        {
            var enumerable = ignoredUrls as IList<string> ?? ignoredUrls.ToList();
            if (ignoredUrls == null || !enumerable.Any())
            {
                return false;
            }

            url = url.TrimStart('~');

            foreach (var ignoredUrl in enumerable)
            {
                var relativePath = ignoredUrl.TrimStart('~').Trim();
                if (string.IsNullOrWhiteSpace(relativePath))
                {
                    continue;
                }

                // Ignore comments
                if (relativePath.StartsWith("#"))
                {
                    continue;
                }

                if (string.Equals(relativePath, url, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual string ComputeCacheKey(ControllerContext controllerContext,
            IEnumerable<KeyValuePair<string, object>> parameters)
        {
            // ReSharper disable once PossibleNullReferenceException
            var url = controllerContext.HttpContext.Request.Url.AbsolutePath;
            return ComputeCacheKey(_settings.Name, url, parameters);
        }

        protected virtual string ComputeCacheKey(string tenant, string absoluteUrl,
            IEnumerable<KeyValuePair<string, object>> parameters)
        {
            var keyBuilder = new StringBuilder();

            keyBuilder.AppendFormat("tenant={0};url={1};", tenant, absoluteUrl.ToLowerInvariant());

            if (parameters != null)
            {
                foreach (var pair in parameters)
                {
                    keyBuilder.AppendFormat("{0}={1};", pair.Key.ToLowerInvariant(),
                        Convert.ToString(pair.Value).ToLowerInvariant());
                }
            }

            return keyBuilder.ToString();
        }

        protected virtual CacheItem GetCacheItem(string key)
        {
            try
            {
                var cacheItem = _cacheStorageProvider.GetCacheItem(key);
                return cacheItem;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An unexpected error occured while reading a cache entry");
            }

            return null;
        }
    }
}