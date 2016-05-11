using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Swartz.Caching;
using Swartz.Environment.Configuration;
using Swartz.Logging;
using Swartz.Mvc.Extensions;
using Swartz.OutputCache.Configuration;
using Swartz.OutputCache.Helpers;
using Swartz.OutputCache.Models;
using Swartz.Services;
using Swartz.Utility.Extensions;
using IFilterProvider = Swartz.Mvc.Filters.IFilterProvider;

namespace Swartz.OutputCache.Filters
{
    public class OutputCacheFilter : IFilterProvider, IActionFilter, IResultFilter, IDisposable
    {
        private static readonly string _refreshKey = "__r";
        private static readonly long Epoch = new DateTime(2014, DateTimeKind.Utc).Ticks;

        private readonly ICacheManager _cacheManager;
        private readonly ICacheSettingsManager _cacheSettingsManager;
        private readonly ICacheService _cacheService;
        private readonly IOutputCacheStorageProvider _cacheStorageProvider;
        private readonly IClock _clock;
        private readonly ShellSettings _settings;
        private readonly ISignals _signals;
        private readonly IWorkContextAccessor _workContextAccessor;
        private string _cacheKey;

        private CacheSettings _cacheSettings;
        private string _invariantCacheKey;
        private DateTime _now;
        private WorkContext _workContext;
        private bool _isCachingRequest;
        private bool _transformRedirect;
        private bool _isDisposed;

        public ILogger Logger { get; set; }

        private CacheSettings CacheSettings
        {
            get
            {
                return _cacheSettings ?? (_cacheSettings = _cacheManager.Get(CacheSettings.CacheKey, true,
                    context =>
                    {
                        context.Monitor(_signals.When(CacheSettings.CacheKey));
                        var settings = _cacheSettingsManager.LoadSettings();
                        if (settings == null)
                        {
                            settings = new CacheSettings
                            {
                                DefaultCacheDuration = 300,
                                DefaultCacheGraceTime = 60,
                                DefaultMaxAge = 0,
                                VaryByQueryStringParameters = null,
                                VaryByRequestHeaders = null,
                                IgnoreNoCache = false,
                                IgnoredUrls = null,
                                CacheAuthenticatedRequests = false,
                                DebugMode = true,
                                VaryByAuthenticationState = false
                            };
                            _cacheSettingsManager.SaveSettings(settings);
                        }
                        return settings;
                    }));
            }
        }


        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // This filter is not reentrant (multiple executions within the same request are
            // not supported) so child actions are ignored completely.
            if (filterContext.IsChildAction)
            {
                return;
            }

            if (!RequestIsCacheable(filterContext))
            {
                return;
            }

            _now = _clock.UtcNow;
            _workContext = _workContextAccessor.GetContext();

            // Computing the cache key after we know that the request is cacheable means that we are only performing this calculation on requests that require it
            _cacheKey = string.Intern(ComputeCacheKey(filterContext, GetCacheKeyParameters(filterContext)));
            _invariantCacheKey = ComputeCacheKey(filterContext, null);

            try
            {
                var allowServeFromCache = filterContext.RequestContext.HttpContext.Request.Headers["Cache-Control"] !=
                                          "no-cache" || CacheSettings.IgnoreNoCache;
                var cacheItem = GetCacheItem(_cacheKey);

                if (allowServeFromCache && cacheItem != null)
                {
                    // Is the cached item in its grace period?
                    if (cacheItem.IsInGracePeriod(_now))
                    {
                        if (Monitor.TryEnter(_cacheKey))
                        {
                            BeginRenderItem(filterContext);
                            return;
                        }
                    }

                    // Cached item is not yet in its grace period, or is already being
                    // rendered by another request; serve it from cache.
                    ServeCachedItem(filterContext, cacheItem);
                    return;
                }

                // No cached item found, or client doesn't want it; acquire the cache key
                // lock to render the item.
                if (Monitor.TryEnter(_cacheKey))
                {
                    // Item might now have been rendered and cached by another request; if so serve it from cache.
                    if (allowServeFromCache)
                    {
                        cacheItem = GetCacheItem(_cacheKey);
                        if (cacheItem != null)
                        {
                            Monitor.Exit(_cacheKey);
                            ServeCachedItem(filterContext, cacheItem);
                            return;
                        }
                    }
                }

                // Either we acquired the cache key lock and the item was still not in cache, or
                // the lock acquisition timed out. In either case render the item.
                BeginRenderItem(filterContext);
            }
            catch
            {
                ReleaseCacheKeyLock();
                throw;
            }
        }

        protected virtual bool TransformRedirect(ActionExecutedContext filterContext)
        {
            // Removes the target of the redirection from cache after a POST.

            if (filterContext.Result == null)
            {
                throw new ArgumentNullException();
            }

            var redirectResult = filterContext.Result as RedirectResult;

            // status code can't be tested at this point, so test the result type instead
            if (redirectResult == null ||
                !filterContext.HttpContext.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var redirectUrl = redirectResult.Url;

            if (filterContext.HttpContext.Request.IsLocalUrl(redirectUrl))
            {
                // Remove all cached versions of the same item.
                var helper = new UrlHelper(filterContext.HttpContext.Request.RequestContext);
                var absolutePath = new Uri(helper.MakeAbsolute(redirectUrl)).AbsolutePath;
                var invariantCacheKey = ComputeCacheKey(_settings.Name, absolutePath, null);
                _cacheService.RemoveByTag(invariantCacheKey);
            }

            // Adding a refresh key so that the redirection doesn't get restored
            // from a cached version on a proxy. This can happen when using public
            // caching, we want to force the client to get a fresh copy of the
            // redirectUrl content.

            if (CacheSettings.DefaultMaxAge > 0)
            {
                var eqIndex = redirectUrl.IndexOf('?');
                var qs = new NameValueCollection();
                if (eqIndex > 0)
                {
                    qs = HttpUtility.ParseQueryString(redirectUrl.Substring(eqIndex));
                }

                // Substract Epoch to get a smaller number.
                var refresh = _now.Ticks - Epoch;
                qs.Remove(_refreshKey);

                qs.Add(_refreshKey, refresh.ToString("x"));
                var querystring = "?" + string.Join("&", Array.ConvertAll(qs.AllKeys, k =>
                    $"{HttpUtility.UrlEncode(k)}={HttpUtility.UrlEncode(qs[k])}"));

                if (eqIndex > 0)
                {
                    redirectUrl = redirectUrl.Substring(0, eqIndex) + querystring;
                }
                else
                {
                    redirectUrl = redirectUrl + querystring;
                }
            }

            filterContext.Result = new RedirectResult(redirectUrl, redirectResult.Permanent);
            filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            return true;
        }

        private void ServeCachedItem(ActionExecutingContext filterContext, CacheItem cacheItem)
        {
            var response = filterContext.HttpContext.Response;
            var request = filterContext.HttpContext.Request;

            // Fix for missing charset in response headers
            response.Charset = response.Charset;

            // Adds some caching information to the output if requested.
            if (CacheSettings.DebugMode)
            {
                response.AddHeader("X-Cached-On", cacheItem.CachedOnUtc.ToString("r"));
                response.AddHeader("X-Cached-Until", cacheItem.ValidUntilUtc.ToString("r"));
            }

            // Shortcut action execution.
            filterContext.Result = new FileContentResult(cacheItem.Output, cacheItem.ContentType);
            response.StatusCode = cacheItem.StatusCode;

            // Add ETag header
            if (HttpRuntime.UsingIntegratedPipeline && response.Headers.Get("ETag") == null && cacheItem.ETag != null)
            {
                response.Headers["ETag"] = cacheItem.ETag;
            }

            // Check ETag in request
            // https://www.w3.org/2005/MWI/BPWG/techs/CachingWithETag.html
            var etag = request.Headers["If-None-Match"];
            if (!string.IsNullOrEmpty(etag))
            {
                if (string.Equals(etag, cacheItem.ETag, StringComparison.Ordinal))
                {
                    // ETag matches the cached item, we return a 304
                    filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.NotModified);
                    return;
                }
            }

            ApplyCacheControl(response);
        }

        private void BeginRenderItem(ActionExecutingContext filtercContext)
        {
            var response = filtercContext.HttpContext.Response;

            ApplyCacheControl(response);

            // Remember that we should intercept the rendered response output.
            _isCachingRequest = true;
        }

        private void ApplyCacheControl(HttpResponseBase response)
        {
            if (CacheSettings.DefaultMaxAge > 0)
            {
                //cacheItem.ValidUntilUtc - _clock.UtcNow;
                var maxAge = TimeSpan.FromSeconds(CacheSettings.DefaultMaxAge);
                if (maxAge.TotalMilliseconds < 0)
                {
                    maxAge = TimeSpan.Zero;
                }
                response.Cache.SetCacheability(HttpCacheability.Public);
                response.Cache.SetMaxAge(maxAge);
            }

            // Keeping this example for later usage.
            // response.DisableUserCache();
            // response.DisableKernelCache();
            // response.Cache.SetOmitVaryStar(true);

            if (CacheSettings.VaryByQueryStringParameters == null)
            {
                response.Cache.VaryByParams["*"] = true;
            }
            else
            {
                foreach (var queryStringParameter in CacheSettings.VaryByQueryStringParameters)
                {
                    response.Cache.VaryByParams[queryStringParameter] = true;
                }
            }

            foreach (var varyByRequestHeader in CacheSettings.VaryByRequestHeaders)
            {
                response.Cache.VaryByHeaders[varyByRequestHeader] = true;
            }
        }

        private void ReleaseCacheKeyLock()
        {
            if (_cacheKey != null && Monitor.IsEntered(_cacheKey))
            {
                Monitor.Exit(_cacheKey);
                _cacheKey = null;
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            _transformRedirect = TransformRedirect(filterContext);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                }

                if (_cacheKey != null && Monitor.IsEntered(_cacheKey))
                {
                    Monitor.Exit(_cacheKey);
                }

                _isDisposed = true;
            }
        }

        ~OutputCacheFilter()
        {
            // Ensure locks are released even after an unexpected exception
            Dispose(false);
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            // This filter is not reentrant (multiple executions within the same request are
            // not supported) so child actions are ignored completely.
            if (filterContext.IsChildAction) return;

            var captureHandlerIsAttached = false;

            try
            {
                if (!_isCachingRequest) return;

                if (!ResponseIsCacheable(filterContext))
                {
                    filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    filterContext.HttpContext.Response.Cache.SetNoStore();
                    filterContext.HttpContext.Response.Cache.SetMaxAge(new TimeSpan(0));
                    return;
                }

                // Capture the response output using a custom filter stream.
                var response = filterContext.HttpContext.Response;
                var captureStream = new CaptureStream(response.Filter);
                response.Filter = captureStream;

                // Add ETag header for the newly created item
                var etag = Guid.NewGuid().ToString("n");
                if (HttpRuntime.UsingIntegratedPipeline)
                {
                    if (response.Headers.Get("ETag") == null)
                    {
                        response.Headers["ETag"] = etag;
                    }
                }

                captureStream.Captured += (output) =>
                {
                    try
                    {
                        // Since this is a callback any call to injected dependencies can result in an Autofac exception: "Instances 
                        // cannot be resolved and nested lifetimes cannot be created from this LifetimeScope as it has already been disposed."
                        // To prevent access to the original lifetime scope a new work context scope should be created here and dependencies
                        // should be resolved from it.

                        using (var scope = _workContextAccessor.CreateWorkContextScope())
                        {
                            var cacheItem = new CacheItem
                            {
                                CachedOnUtc = _now
                            };

                            // Write the rendered item to the cache.
                            var cacheStorageProvider = scope.Resolve<IOutputCacheStorageProvider>();
                            cacheStorageProvider.Set(_cacheKey, cacheItem);

                            // Also add the item tags to the tag cache.
                            var tagCache = scope.Resolve<ITagCache>();
                            foreach (var tag in cacheItem.Tags)
                            {
                                tagCache.Tag(tag, _cacheKey);
                            }
                        }
                    }
                    finally
                    {
                        // Always release the cache key lock when the request ends.
                        ReleaseCacheKeyLock();
                    }
                };

                captureHandlerIsAttached = true;
            }
            finally
            {
                // If the response filter stream capture handler was attached then we'll trust
                // it to release the cache key lock at some point in the future when the stream
                // is flushed; otherwise we'll make sure we'll release it here.
                if (!captureHandlerIsAttached)
                {
                    ReleaseCacheKeyLock();
                }
            }
        }

        protected virtual bool ResponseIsCacheable(ResultExecutedContext filterContext)
        {
            if (filterContext.HttpContext.Request.Url == null)
            {
                return false;
            }

            // Don't cache non-200 responses or results of a redirect.
            var response = filterContext.HttpContext.Response;
            if (response.StatusCode != (int) HttpStatusCode.OK || _transformRedirect)
            {
                return false;
            }

            return true;
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

            // Don't cache ignored URLs.
            if (IsIgnoredUrl(filterContext.RequestContext.HttpContext.Request.AppRelativeCurrentExecutionFilePath,
                CacheSettings.IgnoredUrls))
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
            if (ignoredUrls == null)
            {
                return false;
            }

            var enumerable = ignoredUrls as IList<string> ?? ignoredUrls.ToList();
            if (!enumerable.Any())
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

        protected virtual IDictionary<string, object> GetCacheKeyParameters(ActionExecutingContext filterContext)
        {
            var result = new Dictionary<string, object>();

            // Vary by action parameters.
            foreach (var p in filterContext.ActionParameters)
            {
                result.Add("PARAM:" + p.Key, p.Value);
            }

            // Vary by scheme.
            // ReSharper disable once PossibleNullReferenceException
            result.Add("scheme", filterContext.RequestContext.HttpContext.Request.Url.Scheme);

            // Vary by configured query string parameters.
            var queryString = filterContext.RequestContext.HttpContext.Request.QueryString;
            foreach (var key in queryString.AllKeys)
            {
                if (key == null ||
                    (CacheSettings.VaryByQueryStringParameters != null &&
                     !CacheSettings.VaryByQueryStringParameters.Contains(key)))
                {
                    continue;
                }

                result[key] = queryString[key];
            }

            if (CacheSettings.VaryByRequestHeaders != null)
            {
                var requestHeaders = filterContext.RequestContext.HttpContext.Request.Headers;
                foreach (var varyByRequestHeader in CacheSettings.VaryByRequestHeaders)
                {
                    if (requestHeaders[varyByRequestHeader] != null)
                    {
                        result["HEADER:" + varyByRequestHeader] = requestHeaders[varyByRequestHeader];
                    }
                }
            }

            // Vary by authentication state if configured.
            if (CacheSettings.VaryByAuthenticationState)
            {
                result["auth"] =
                    filterContext.HttpContext.User.Identity.IsAuthenticated.ToString().ToLowerInvariant();
            }

            return result;
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