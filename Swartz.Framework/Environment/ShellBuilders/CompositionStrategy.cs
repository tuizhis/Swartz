using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Autofac.Core;
using Swartz.Environment.Configuration;
using Swartz.Environment.Extensions;
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
                    .Select(r => _assemblyLoader.Load(Path.GetFileNameWithoutExtension(r.FullName)))
                    .ToArray();

            var excludedTypes = (HashSet<string>) GetExcludedTypes(assemblies);
            var modules = BuildBlueprint(assemblies, IsModule, BuildDependency, excludedTypes);
            var dependencies = BuildBlueprint(assemblies, IsDependency, BuildDependency, excludedTypes);
            var controllers = BuildBlueprint(assemblies, IsController, BuildController, excludedTypes);
            var httpControllers = BuildBlueprint(assemblies, IsHttpController, BuildController, excludedTypes);

            var result = new ShellBlueprint
            {
                Controllers = controllers,
                HttpControllers = httpControllers,
                Dependencies = dependencies.Concat(modules)
            };

            return result;
        }

        private static ShellBlueprintItem BuildDependency(Type type)
        {
            return new ShellBlueprintItem
            {
                Type = type
            };
        }

        private static ControllerBlueprint BuildController(Type type)
        {
            var areaName = typeof(IHttpController).IsAssignableFrom(type) ? GetHttpAreaName(type) : GetAreaName(type);

            var controllerName = GetControllerName(type);

            return new ControllerBlueprint
            {
                Type = type,
                AreaName = areaName,
                ControllerName = controllerName
            };
        }

        private static IEnumerable<T> BuildBlueprint<T>(IEnumerable<Assembly> assemblies, Func<Type, bool> predicate,
            Func<Type, T> selector, IEnumerable<string> excludedTypes)
        {
            return
                assemblies.SelectMany(
                    a =>
                        a.GetExportedTypes()
                            .Where(type => type.IsClass && !type.IsAbstract)
                            .Where(predicate)
                            .Where(type => !excludedTypes.Contains(type.FullName))
                            .Select(selector)).ToArray();
        }

        private static IEnumerable<string> GetExcludedTypes(IEnumerable<Assembly> assemblies)
        {
            var excludedTypes = new HashSet<string>();

            // Identify replaced types.
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetExportedTypes())
                {
                    foreach (
                        SwartzSuppressDependencyAttribute attribute in
                            type.GetCustomAttributes(typeof(SwartzSuppressDependencyAttribute), false))
                    {
                        excludedTypes.Add(attribute.FullName);
                    }
                }
            }

            return excludedTypes;
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