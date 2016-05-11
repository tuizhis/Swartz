using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Web;
using Autofac;
using Swartz.Logging;
using Swartz.Mvc;
using Swartz.Mvc.Extensions;

namespace Swartz.Environment
{
    public class WorkContextAccessor : ILogicalWorkContextAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILifetimeScope _lifetimeScope;
        // a different symbolic key is used for each tenant.
        // this guarantees the correct accessor is being resolved.
        private readonly object _workContextKey = new object();
        private readonly string _workContextSlot;

        public WorkContextAccessor(IHttpContextAccessor httpContextAccessor, ILifetimeScope lifetimeScope)
        {
            _httpContextAccessor = httpContextAccessor;
            _lifetimeScope = lifetimeScope;
            _workContextSlot = "WorkContext." + Guid.NewGuid().ToString("n");
        }

        public WorkContext GetContext(HttpContextBase httpContext)
        {
            var context = httpContext.Items[_workContextKey] as WorkContext;
            if (context == null)
            {
                return GetLogicalContext();
            }

            return context;
        }

        public IWorkContextScope CreateWorkContextScope(HttpContextBase httpContext)
        {
            var workLifetime = _lifetimeScope.BeginLifetimeScope("work");
            var events = workLifetime.Resolve<IEnumerable<IWorkContextEvents>>().ToList();
            events.Invoke(e => e.Started(), NullLogger.Instance);

            if (!httpContext.IsBackgroundContext())
            {
                return new HttpContextScopeImplementation(events, workLifetime, httpContext, _workContextKey);
            }

            return new CallContextScopeImplementation(events, workLifetime, _workContextSlot);
        }

        public WorkContext GetContext()
        {
            var context = _httpContextAccessor.Current();
            return GetContext(context);
        }

        public IWorkContextScope CreateWorkContextScope()
        {
            var httpContext = _httpContextAccessor.Current();
            return CreateWorkContextScope(httpContext);
        }

        public WorkContext GetLogicalContext()
        {
            var context = CallContext.LogicalGetData(_workContextSlot) as ObjectHandle;
            return context?.Unwrap() as WorkContext;
        }

        private class HttpContextScopeImplementation : IWorkContextScope
        {
            private readonly Action _disposer;

            public HttpContextScopeImplementation(IEnumerable<IWorkContextEvents> events, ILifetimeScope lifetimeScope,
                HttpContextBase httpContext, object workContextKey)
            {
                WorkContext = lifetimeScope.Resolve<WorkContext>();
                httpContext.Items[workContextKey] = WorkContext;

                _disposer = () =>
                {
                    events.Invoke(e => e.Finished(), NullLogger.Instance);
                    httpContext.Items.Remove(workContextKey);
                    lifetimeScope.Dispose();
                };
            }

            void IDisposable.Dispose()
            {
                _disposer();
            }

            public WorkContext WorkContext { get; }

            public TService Resolve<TService>()
            {
                return WorkContext.Resolve<TService>();
            }

            public bool TryResolve<TService>(out TService service)
            {
                return WorkContext.TryResolve(out service);
            }
        }

        private class CallContextScopeImplementation : IWorkContextScope
        {
            private readonly Action _disposer;

            public CallContextScopeImplementation(IEnumerable<IWorkContextEvents> events, ILifetimeScope lifetimeScope,
                string workContextSlot)
            {
                CallContext.LogicalSetData(workContextSlot, null);

                WorkContext = lifetimeScope.Resolve<WorkContext>();
                var httpContext = lifetimeScope.Resolve<HttpContextBase>();
                WorkContext.HttpContext = httpContext;

                CallContext.LogicalSetData(workContextSlot, new ObjectHandle(WorkContext));

                _disposer = () =>
                {
                    events.Invoke(e => e.Finished(), NullLogger.Instance);
                    CallContext.FreeNamedDataSlot(workContextSlot);
                    lifetimeScope.Dispose();
                };
            }

            void IDisposable.Dispose()
            {
                _disposer();
            }

            public WorkContext WorkContext { get; }

            public TService Resolve<TService>()
            {
                return WorkContext.Resolve<TService>();
            }

            public bool TryResolve<TService>(out TService service)
            {
                return WorkContext.TryResolve(out service);
            }
        }
    }
}