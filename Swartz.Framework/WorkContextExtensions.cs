using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;

namespace Swartz
{
    public static class WorkContextExtensions
    {
        public static WorkContext GetContext(this IWorkContextAccessor workContextAccessor,
            ControllerContext controllerContext)
        {
            return workContextAccessor.GetContext(controllerContext.RequestContext.HttpContext);
        }

        public static WorkContext GetLogicalContext(this IWorkContextAccessor workContextAccessor)
        {
            var wca = workContextAccessor as ILogicalWorkContextAccessor;
            return wca?.GetLogicalContext();
        }

        public static WorkContext GetWorkContext(this RequestContext requestContext)
        {
            var routeData = requestContext?.RouteData;
            if (routeData?.DataTokens == null)
                return null;

            object workContextValue;
            if (!routeData.DataTokens.TryGetValue("IWorkContextAccessor", out workContextValue))
            {
                workContextValue = FindWorkContextInParent(routeData);
            }

            var workContextAccessor = workContextValue as IWorkContextAccessor;
            return workContextAccessor?.GetContext(requestContext.HttpContext);
        }

        public static WorkContext GetWorkContext(this HttpControllerContext controllerContext)
        {
            var routeData = controllerContext?.RouteData;
            if (routeData?.Values == null)
                return null;

            object workContextValue;
            if (!routeData.Values.TryGetValue("IWorkContextAccessor", out workContextValue))
            {
                return null;
            }

            var workContextAccessor = workContextValue as IWorkContextAccessor;
            return workContextAccessor?.GetContext();
        }

        private static object FindWorkContextInParent(RouteData routeData)
        {
            object parentViewContextValue;
            if (!routeData.DataTokens.TryGetValue("ParentActionViewContext", out parentViewContextValue)
                || !(parentViewContextValue is ViewContext))
            {
                return null;
            }

            var parentRouteData = ((ViewContext) parentViewContextValue).RouteData;
            if (parentRouteData?.DataTokens == null)
                return null;

            object workContextValue;
            if (!parentRouteData.DataTokens.TryGetValue("IWorkContextAccessor", out workContextValue))
            {
                workContextValue = FindWorkContextInParent(parentRouteData);
            }

            return workContextValue;
        }

        public static WorkContext GetWorkContext(this ControllerContext controllerContext)
        {
            if (controllerContext == null)
                return null;

            return GetWorkContext(controllerContext.RequestContext);
        }

        public static IWorkContextScope CreateWorkContextScope(this ILifetimeScope lifetimeScope,
            HttpContextBase httpContext)
        {
            return lifetimeScope.Resolve<IWorkContextAccessor>().CreateWorkContextScope(httpContext);
        }

        public static IWorkContextScope CreateWorkContextScope(this ILifetimeScope lifetimeScope)
        {
            return lifetimeScope.Resolve<IWorkContextAccessor>().CreateWorkContextScope();
        }
    }
}