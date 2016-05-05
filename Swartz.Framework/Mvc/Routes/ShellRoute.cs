using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Swartz.Environment.Configuration;
using Swartz.Mvc.Extensions;

namespace Swartz.Mvc.Routes
{
    public class ShellRoute : RouteBase, IRouteWithArea
    {
        private readonly Func<IDictionary<string, object>, Task> _pipeline;
        private readonly RouteBase _route;
        private readonly ShellSettings _shellSettings;
        private readonly IWorkContextAccessor _workContextAccessor;

        public ShellRoute(RouteBase route, ShellSettings shellSettings, IWorkContextAccessor workContextAccessor,
            Func<IDictionary<string, object>, Task> pipeline)
        {
            _route = route;
            _shellSettings = shellSettings;
            _pipeline = pipeline;
            _workContextAccessor = workContextAccessor;

            Area = route.GetAreaName();
        }

        public SessionStateBehavior SessionState { get; set; }

        public string ShellSettingsName => _shellSettings.Name;

        public bool IsHttpRoute { get; set; }

        public string Area { get; }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            var routeData = _route.GetRouteData(httpContext);
            if (routeData == null)
            {
                return null;
            }

            // if a StopRoutingHandler was registered, no need to do anything further
            if (routeData.RouteHandler is StopRoutingHandler)
            {
                return routeData;
            }

            // otherwise wrap handler and return it
            routeData.RouteHandler = new RouteHandler(_workContextAccessor, routeData.RouteHandler, SessionState,
                _pipeline);
            routeData.DataTokens["IWorkContextAccessor"] = _workContextAccessor;

            if (IsHttpRoute)
            {
                routeData.Values["IWorkContextAccessor"] = _workContextAccessor;
            }

            return routeData;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            return _route.GetVirtualPath(requestContext, values);
        }

        private class RouteHandler : IRouteHandler
        {
            private readonly Func<IDictionary<string, object>, Task> _pipeline;
            private readonly IRouteHandler _routeHandler;
            private readonly SessionStateBehavior _sessionStateBehavior;
            private readonly IWorkContextAccessor _workContextAccessor;

            public RouteHandler(IWorkContextAccessor workContextAccessor, IRouteHandler routeHandler,
                SessionStateBehavior sessionStateBehavior, Func<IDictionary<string, object>, Task> pipeline)
            {
                _workContextAccessor = workContextAccessor;
                _routeHandler = routeHandler;
                _sessionStateBehavior = sessionStateBehavior;
                _pipeline = pipeline;
            }

            public IHttpHandler GetHttpHandler(RequestContext requestContext)
            {
                var httpHandler = _routeHandler.GetHttpHandler(requestContext);

                requestContext.HttpContext.SetSessionStateBehavior(_sessionStateBehavior);

                if (httpHandler is IHttpAsyncHandler)
                {
                    return new HttpAsyncHandler(_workContextAccessor, httpHandler, _pipeline);
                }

                return new HttpHandler(_workContextAccessor, httpHandler);
            }
        }

        private class HttpHandler : IHttpHandler, IRequiresSessionState, IHasRequestContext
        {
            private readonly IHttpHandler _httpHandler;
            private readonly IWorkContextAccessor _workContextAccessor;

            public HttpHandler(IWorkContextAccessor workContextAccessor, IHttpHandler httpHandler)
            {
                _workContextAccessor = workContextAccessor;
                _httpHandler = httpHandler;
            }

            public RequestContext RequestContext
            {
                get
                {
                    var mvcHandler = _httpHandler as MvcHandler;
                    return mvcHandler?.RequestContext;
                }
            }

            public bool IsReusable => false;

            public void ProcessRequest(HttpContext context)
            {
                using (_workContextAccessor.CreateWorkContextScope(new HttpContextWrapper(context)))
                {
                    _httpHandler.ProcessRequest(context);
                }
            }
        }

        private class HttpAsyncHandler : HttpTaskAsyncHandler, IRequiresSessionState, IHasRequestContext
        {
            private readonly IHttpAsyncHandler _httpAsyncHandler;
            private readonly Func<IDictionary<string, object>, Task> _pipeline;
            private readonly IWorkContextAccessor _workContextAccessor;

            public HttpAsyncHandler(IWorkContextAccessor workContextAccessor, IHttpHandler httpHandler,
                Func<IDictionary<string, object>, Task> env)
            {
                _workContextAccessor = workContextAccessor;
                _httpAsyncHandler = httpHandler as IHttpAsyncHandler;
                _pipeline = env;
            }

            public RequestContext RequestContext
            {
                get
                {
                    var mvcHandler = _httpAsyncHandler as MvcHandler;
                    return mvcHandler?.RequestContext;
                }
            }

            public override void ProcessRequest(HttpContext context)
            {
                throw new NotImplementedException();
            }

            public override async Task ProcessRequestAsync(HttpContext context)
            {
                using (_workContextAccessor.CreateWorkContextScope(new HttpContextWrapper(context)))
                {
                    var environment = context.Items["owin.Environment"] as IDictionary<string, object> ??
                                      new Dictionary<string, object>();

                    environment["orchard.Handler"] = new Func<Task>(async () =>
                    {
                        await Task.Factory.FromAsync(
                            _httpAsyncHandler.BeginProcessRequest,
                            _httpAsyncHandler.EndProcessRequest,
                            context,
                            null);
                    });

                    await _pipeline.Invoke(environment);
                }
            }
        }
    }
}