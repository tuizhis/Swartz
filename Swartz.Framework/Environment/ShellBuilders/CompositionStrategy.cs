using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Autofac.Core;
using Swartz.Environment.Configuration;
using Swartz.Environment.ShellBuilders.Models;

namespace Swartz.Environment.ShellBuilders
{
    public class CompositionStrategy : ICompositionStrategy
    {
        private const string DefaultHttpApiArea = "Common";
        private readonly IAssemblyLoader _assemblyLoader;

        public CompositionStrategy(IAssemblyLoader assemblyLoader)
        {
            _assemblyLoader = assemblyLoader;
        }

        public ShellBlueprint Compose(ShellSettings settings)
        {
            var binPath = HostingEnvironment.MapPath("~/bin");
            if (string.IsNullOrWhiteSpace(binPath))
            {
                throw new DirectoryNotFoundException("没有找到应用程序bin文件夹");
            }

            var assemblies =
                new DirectoryInfo(binPath).GetFiles("*.dll", SearchOption.TopDirectoryOnly)
                    .Concat(new DirectoryInfo(binPath).GetFiles("*.exe", SearchOption.TopDirectoryOnly))
                    .Where(x => !Path.GetFileNameWithoutExtension(x.FullName).Contains("vshost"))
                    .Select(r => _assemblyLoader.Load(Path.GetFileNameWithoutExtension(r.FullName)))
                    .ToArray();

            var modules = new List<ShellBlueprintItem>();
            var dependencies = new List<ShellBlueprintItem>();
            var controllers = new List<ControllerBlueprint>();
            var httpControllers = new List<ControllerBlueprint>();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract).ToList();
                var m = types.Where(IsModule).Select(t => new ShellBlueprintItem
                {
                    Type = t
                });
                modules.AddRange(m);

                var d = types.Where(IsDependency).Select(x => new ShellBlueprintItem
                {
                    Type = x
                });
                dependencies.AddRange(d);

                var c = types.Where(IsController).Select(t => new ControllerBlueprint
                {
                    Type = t,
                    AreaName = GetAreaName(t),
                    ControllerName = GetControllerName(t)
                });
                controllers.AddRange(c);

                var h = types.Where(IsHttpController).Select(x => new ControllerBlueprint
                {
                    Type = x,
                    AreaName = GetHttpAreaName(x),
                    ControllerName = GetControllerName(x)
                });
                httpControllers.AddRange(h);
            }

            var result = new ShellBlueprint
            {
                Controllers = controllers,
                HttpControllers = httpControllers,
                Modules = modules,
                Dependencies = dependencies
            };

            return result;
        }

        private static bool IsModule(Type type)
        {
            return typeof(IModule).IsAssignableFrom(type);
        }

        private static bool IsController(Type type)
        {
            return typeof(IController).IsAssignableFrom(type);
        }

        private static bool IsHttpController(Type type)
        {
            return typeof(IHttpController).IsAssignableFrom(type);
        }

        private static bool IsDependency(Type type)
        {
            return typeof(IDependency).IsAssignableFrom(type);
        }

        private static string GetControllerName(Type t)
        {
            var controllerName = t.Name;
            if (controllerName.EndsWith("Controller"))
            {
                controllerName = controllerName.Substring(0, controllerName.Length - "Controller".Length);
            }
            return controllerName;
        }

        private static string GetAreaName(Type t)
        {
            var ns = t.Namespace;
            if (ns != null && ns.Contains(".Areas."))
            {
                ns = ns.Replace(".Controllers", "");
                return ns.Substring(ns.LastIndexOf(".", StringComparison.Ordinal) + 1);
            }

            return string.Empty;
        }

        private static string GetHttpAreaName(Type t)
        {
            var ns = t.Namespace;
            if (ns != null && ns.Contains(".Areas."))
            {
                ns = ns.Replace(".Controllers", "");
                return ns.Substring(ns.LastIndexOf(".", StringComparison.Ordinal) + 1);
            }

            return DefaultHttpApiArea;
        }
    }
}