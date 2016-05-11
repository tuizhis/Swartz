using System;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Swartz.Caching;
using Swartz.Environment.Configuration;
using Swartz.Environment.ShellBuilders;
using Swartz.Events;
using Swartz.FileSystems.AppData;
using Swartz.FileSystems.VirtualPath;
using Swartz.Logging;
using Swartz.Mvc;
using Swartz.Mvc.Filters;
using Swartz.Services;
using Swartz.WebApi;
using Swartz.WebApi.Filters;

namespace Swartz.Environment
{
    public static class SwartzStarter
    {
        public static IContainer CreateHostContainer(Action<ContainerBuilder> registrations, HttpApplication application)
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new CollectionOrderModule());
            builder.RegisterModule(new LoggingModule());
            builder.RegisterModule(new EventsModule());
            builder.RegisterModule(new CacheModule());

            // a single default host implementation is needed for bootstrapping a web app domain
            builder.RegisterType<DefaultSwartzEventBus>().As<IEventBus>().SingleInstance();
            builder.RegisterType<DefaultCacheHolder>().As<ICacheHolder>().SingleInstance();
            builder.RegisterType<DefaultCacheContextAccessor>().As<ICacheContextAccessor>().SingleInstance();
            builder.RegisterType<DefaultHostEnvironment>().As<IHostEnvironment>().SingleInstance();
            builder.RegisterType<DefaultHostLocalRestart>().As<IHostLocalRestart>().SingleInstance();
            builder.RegisterType<AppDataFolderRoot>().As<IAppDataFolderRoot>().SingleInstance();
            builder.RegisterType<DefaultAssemblyLoader>().As<IAssemblyLoader>().SingleInstance();
            builder.RegisterType<AppDomainAssemblyNameResolver>().As<IAssemblyNameResolver>().SingleInstance();
            builder.RegisterType<GacAssemblyNameResolver>().As<IAssemblyNameResolver>().SingleInstance();
            builder.RegisterType<SwartzFrameworkAssemblyNameResolver>().As<IAssemblyNameResolver>().SingleInstance();
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
            builder.RegisterType<ApplicationEnvironment>().As<IApplicationEnvironment>().SingleInstance();

            RegisterVolatileProvider<AppDataFolder, IAppDataFolder>(builder);
            RegisterVolatileProvider<Clock, IClock>(builder);
            RegisterVolatileProvider<DefaultVirtualPathMonitor, IVirtualPathMonitor>(builder);

            builder.RegisterType<DefaultWebHost>().As<IWebHost>().SingleInstance();
            builder.RegisterType<ShellSettingsManager>().As<IShellSettingsManager>().SingleInstance();
            builder.RegisterType<ShellContextFactory>().As<IShellContextFactory>().SingleInstance();
            builder.RegisterType<CompositionStrategy>().As<ICompositionStrategy>().SingleInstance();
            builder.RegisterType<ShellContainerFactory>().As<IShellContainerFactory>().SingleInstance();

            builder.RegisterType<DefaultWebShell>().As<IWebShell>().InstancePerMatchingLifetimeScope("shell");

            builder.Register(ctx => application.Application).SingleInstance();
            RouteTable.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            builder.Register(ctx => RouteTable.Routes).SingleInstance();
            builder.Register(ctx => ModelBinders.Binders).SingleInstance();

            registrations?.Invoke(builder);

            var container = builder.Build();

            ControllerBuilder.Current.SetControllerFactory(new SwartzControllerFactory());
            FilterProviders.Providers.Add(new SwartzFilterProvider());

            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerSelector), new DefaultHttpControllerSelector(GlobalConfiguration.Configuration));
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerActivator), new DefaultSwartzWebApiHttpControllerActivator(GlobalConfiguration.Configuration));
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            GlobalConfiguration.Configuration.Filters.Add(new SwartzApiActionFilterDispatcher());
            GlobalConfiguration.Configuration.Filters.Add(new SwartzApiExceptionFilterDispatcher());
            GlobalConfiguration.Configuration.Filters.Add(new SwartzApiAuthorizationFilterDispatcher());

            var hostContainer = new DefaultSwartzHostContainer(container);
            WebHostContainerRegistry.RegisterHostContainer(hostContainer);

            return container;
        }

        public static IWebHost CreateHost(Action<ContainerBuilder> registrations, HttpApplication application)
        {
            var container = CreateHostContainer(registrations, application);
            return container.Resolve<IWebHost>();
        }

        private static void RegisterVolatileProvider<TRegister, TService>(ContainerBuilder builder)
            where TService : IVolatileProvider
        {
            builder.RegisterType<TRegister>()
                .As<TService>()
                .As<IVolatileProvider>()
                .SingleInstance();
        }
    }
}