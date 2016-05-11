using System;
using System.Web;
using Autofac;

namespace Swartz.Mvc
{
    public class HttpContextAccessor : IHttpContextAccessor
    {
        private readonly ILifetimeScope _lifetimeScope;
        private HttpContextBase _httpContext;
        private IWorkContextAccessor _wca;

        public HttpContextAccessor(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public HttpContextBase Current()
        {
            var httpContext = GetStaticProperty();

            if (!IsBackgroundHttpContext(httpContext))
            {
                return new HttpContextWrapper(httpContext);
            }

            if (_httpContext != null)
            {
                return _httpContext;
            }

            if (_wca == null && _lifetimeScope.IsRegistered<IWorkContextAccessor>())
            {
                _wca = _lifetimeScope.Resolve<IWorkContextAccessor>();
            }

            var workContext = _wca?.GetLogicalContext();
            return workContext?.HttpContext;
        }

        public void Set(HttpContextBase httpContext)
        {
            _httpContext = httpContext;
        }

        private static HttpContext GetStaticProperty()
        {
            var httpContext = HttpContext.Current;
            if (httpContext == null)
            {
                return null;
            }

            try
            {
                // The "Request" property throws at application startup on IIS integrated pipeline mode.
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (httpContext.Request == null)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return httpContext;
        }

        private static bool IsBackgroundHttpContext(HttpContext httpContext)
        {
            return httpContext == null || httpContext.Items.Contains(MvcModule.IsBackgroundHttpContextKey);
        }
    }
}