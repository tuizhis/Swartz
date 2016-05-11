using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Instrumentation;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Swartz.Mvc.Extensions;
using Swartz.Mvc.Routes;

namespace Swartz.Mvc
{
    public class MvcModule : Module
    {
        public const string IsBackgroundHttpContextKey = "IsBackgroundHttpContext";

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ShellRoute>().InstancePerDependency();

            builder.Register(HttpContextBaseFactory).As<HttpContextBase>().InstancePerDependency();
            builder.Register(RequestContextFactory).As<RequestContext>().InstancePerDependency();
            builder.Register(UrlHelperFactory).As<UrlHelper>().InstancePerDependency();
        }

        private static HttpContextBase HttpContextBaseFactory(IComponentContext context)
        {
            var httpContext = context.Resolve<IHttpContextAccessor>().Current();
            if (httpContext != null)
            {
                return httpContext;
            }

            var httpContextBase = new HttpContextPlaceholder(() => "http://localhost");
            return httpContextBase;
        }

        private static RequestContext RequestContextFactory(IComponentContext context)
        {
            var httpContext = HttpContextBaseFactory(context);

            if (!httpContext.IsBackgroundContext())
            {
                var mvcHandler = httpContext.Handler as MvcHandler;
                if (mvcHandler != null)
                {
                    return mvcHandler.RequestContext;
                }

                var hasRequestContext = httpContext.Handler as IHasRequestContext;
                if (hasRequestContext?.RequestContext != null)
                {
                    return hasRequestContext.RequestContext;
                }
            }
            else if (httpContext is HttpContextPlaceholder)
            {
                return ((HttpContextPlaceholder) httpContext).RequestContext;
            }

            return new RequestContext(httpContext, new RouteData());
        }

        private static UrlHelper UrlHelperFactory(IComponentContext context)
        {
            return new UrlHelper(context.Resolve<RequestContext>(), context.Resolve<RouteCollection>());
        }

        /// <summary>
        ///     Standin context for background tasks.
        /// </summary>
        public class HttpContextPlaceholder : HttpContextBase, IDisposable
        {
            private readonly Lazy<string> _baseUrl;
            private HttpContext _httpContext;
            private HttpRequestPlaceholder _request;

            public HttpContextPlaceholder(Func<string> baseUrl)
            {
                _baseUrl = new Lazy<string>(baseUrl);
            }

            public override HttpRequestBase Request
            {
                // Note: To fully resolve the baseUrl, some factories are needed (HttpContextBase, RequestContext...),
                // so, doing this in such a factory creates a circular dependency (see HttpContextBase factory comments).

                // When rendering a view in a background task, an Html Helper can access HttpContext.Current directly,
                // so, here we create a fake HttpContext based on the baseUrl, and use it to update HttpContext.Current.
                // We cannot do this before in a factory (see note above), anyway, by doing this on each Request access,
                // we have a better chance to maintain the HttpContext.Current state even with some asynchronous code.
                get
                {
                    if (_httpContext == null)
                    {
                        var httpContext = new HttpContext(new HttpRequest("", _baseUrl.Value, ""),
                            new HttpResponse(new StringWriter()));
                        httpContext.Items[IsBackgroundHttpContextKey] = true;
                        _httpContext = httpContext;
                    }

                    if (HttpContext.Current != _httpContext)
                        HttpContext.Current = _httpContext;

                    return _request ?? (_request = new HttpRequestPlaceholder(this, _baseUrl));
                }
            }

            internal RequestContext RequestContext
            {
                // Uses the Request object but without creating an HttpContext which would need to resolve the baseUrl,
                // so, can be used by the RequestContext factory without creating a circular dependency (see note above).
                get
                {
                    if (_request == null)
                    {
                        _request = new HttpRequestPlaceholder(this, _baseUrl);
                    }
                    return _request.RequestContext;
                }
            }

            public override HttpSessionStateBase Session => new HttpSessionStatePlaceholder();

            public override IHttpHandler Handler { get; set; }

            public override HttpResponseBase Response => new HttpResponsePlaceholder();

            public override IDictionary Items { get; } = new Dictionary<object, object>();

            public override PageInstrumentationService PageInstrumentation => new PageInstrumentationService();

            public override Cache Cache => HttpRuntime.Cache;

            public override HttpServerUtilityBase Server => new HttpServerUtilityPlaceholder();

            public void Dispose()
            {
                _httpContext = null;
                if (HttpContext.Current != null)
                    HttpContext.Current = null;
            }

            public override object GetService(Type serviceType)
            {
                return null;
            }
        }

        public class HttpSessionStatePlaceholder : HttpSessionStateBase
        {
            public override object this[string name] => null;
        }

        public class HttpResponsePlaceholder : HttpResponseBase
        {
            public override HttpCookieCollection Cookies => new HttpCookieCollection();

            public override string ApplyAppPathModifier(string virtualPath)
            {
                return virtualPath;
            }
        }

        /// <summary>
        ///     standin context for background tasks.
        /// </summary>
        public class HttpRequestPlaceholder : HttpRequestBase
        {
            private readonly Lazy<string> _baseUrl;
            private readonly HttpContextBase _httpContext;
            private RequestContext _requestContext;
            private Uri _uri;

            public HttpRequestPlaceholder(HttpContextBase httpContext, Lazy<string> baseUrl)
            {
                _httpContext = httpContext;
                _baseUrl = baseUrl;
            }

            public override RequestContext RequestContext
                => _requestContext ?? (_requestContext = new RequestContext(_httpContext, new RouteData()));

            public override NameValueCollection QueryString { get; } = new NameValueCollection();

            public override string RawUrl => Url.OriginalString;

            /// <summary>
            ///     anonymous identity provided for background task.
            /// </summary>
            public override bool IsAuthenticated => false;

            /// <summary>
            ///     Create an anonymous ID the same way as ASP.NET would.
            ///     Some users of an HttpRequestPlaceHolder object could expect this.
            /// </summary>
            public override string AnonymousID => Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture);

            // empty collection provided for background operation
            public override NameValueCollection Form => new NameValueCollection();

            public override Uri Url => _uri ?? (_uri = new Uri(_baseUrl.Value));

            public override NameValueCollection Headers => new NameValueCollection {{"Host", Url.Authority}};

            public override string HttpMethod => "";

            public override NameValueCollection Params => new NameValueCollection();

            public override string AppRelativeCurrentExecutionFilePath => "~/";

            public override string ApplicationPath => Url.LocalPath;

            public override NameValueCollection ServerVariables => new NameValueCollection
            {
                {"SERVER_PORT", Url.Port.ToString(CultureInfo.InvariantCulture)},
                {"HTTP_HOST", Url.Authority.ToString(CultureInfo.InvariantCulture)}
            };

            public override HttpCookieCollection Cookies => new HttpCookieCollection();

            public override bool IsLocal => true;

            public override string Path => "/";

            public override string UserAgent => "Placeholder";

            public override string UserHostAddress => "127.0.0.1";

            public override string[] UserLanguages => new string[0];

            public override HttpBrowserCapabilitiesBase Browser => new HttpBrowserCapabilitiesPlaceholder();
        }

        public class HttpBrowserCapabilitiesPlaceholder : HttpBrowserCapabilitiesBase
        {
            public override string this[string key] => "";

            public override bool IsMobileDevice => false;
            public override string Browser => "Placeholder";
            public override bool Cookies => true;
            public override ArrayList Browsers => new ArrayList();
        }

        public class HttpServerUtilityPlaceholder : HttpServerUtilityBase
        {
            public override int ScriptTimeout { get; set; }

            public override string MapPath(string path)
            {
                return HostingEnvironment.MapPath(path);
            }
        }
    }
}