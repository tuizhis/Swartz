using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.Owin.Builder;
using Swartz.Environment.Configuration;
using Swartz.FileSystems.AppData;
using Swartz.Logging;
using Swartz.Mvc.ModelBinders;
using Swartz.Mvc.Routes;
using Swartz.Owin;
using Swartz.Utility.Extensions;
using Swartz.WebApi.Routes;

namespace Swartz.Environment
{
    public class DefaultWebShell : IWebShell
    {
        private readonly IAppDataFolder _appDataFolder;
        private readonly HttpApplicationState _application;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IEnumerable<IHttpRouteProvider> _httpRouteProviders;
        private readonly IEnumerable<IModelBinderProvider> _modelBinderProviders;
        private readonly IModelBinderPublisher _modelBinderPublisher;
        private readonly IEnumerable<IOwinMiddlewareProvider> _owinMiddlewareProviders;
        private readonly IEnumerable<IRouteProvider> _routeProviders;
        private readonly IRoutePublisher _routePublisher;
        private readonly ShellSettings _settings;

        public DefaultWebShell(IRoutePublisher routePublisher, IEnumerable<IHttpRouteProvider> httpRouteProviders,
            IEnumerable<IRouteProvider> routeProviders, IEnumerable<IOwinMiddlewareProvider> owinMiddlewareProviders,
            IEnumerable<IModelBinderProvider> modelBinderProviders, IModelBinderPublisher modelBinderPublisher,
            HttpApplicationState application, ShellSettings settings, IAppDataFolder appDataFolder,
            IHostEnvironment hostEnvironment)
        {
            _routePublisher = routePublisher;
            _httpRouteProviders = httpRouteProviders;
            _routeProviders = routeProviders;
            _owinMiddlewareProviders = owinMiddlewareProviders;
            _modelBinderProviders = modelBinderProviders;
            _modelBinderPublisher = modelBinderPublisher;
            _application = application;
            _settings = settings;
            _appDataFolder = appDataFolder;
            _hostEnvironment = hostEnvironment;
        }

        public ILogger Logger { get; set; }

        public void Activate()
        {
            var appBuilder = new AppBuilder();
            appBuilder.Properties["host.AppName"] = "Swartz";

            var orderedMiddlewares =
                _owinMiddlewareProviders.SelectMany(p => p.GetOwinMiddlewares())
                    .OrderBy(o => o.Priority, new FlatPositionComparer());

            foreach (var middleware in orderedMiddlewares)
            {
                middleware.Configure(appBuilder);
            }

            // Register the Feiniu middleware after all others.
            appBuilder.UseSwartz();

            var pipeline = appBuilder.Build();
            var allRoutes = new List<RouteDescriptor>();
            allRoutes.AddRange(_routeProviders.SelectMany(provider => provider.GetRoutes()));
            allRoutes.AddRange(_httpRouteProviders.SelectMany(provider => provider.GetRoutes()));

            _routePublisher.Publish(allRoutes, pipeline);
            _modelBinderPublisher.Publish(_modelBinderProviders.SelectMany(provider => provider.GetModelBinders()));

            var monitorPath =
                Path.Combine(
                    _appDataFolder.MapPath(Path.Combine("Sites", _settings.Name.ToSafeDirectoryName())));
            var fsw = new FileSystemWatcher(monitorPath)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };
            fsw.Changed += FileChanged;
            fsw.Deleted += FileChanged;

            _application["FileSystemWatcher"] = fsw;
        }

        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            Logger.Warning("应用程序域重启，原因：Settings.txt文件被修改");
            _hostEnvironment.RestartAppDomain();
        }
    }
}