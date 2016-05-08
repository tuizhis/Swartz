using System;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Autofac;
using Autofac.Core;
using Autofac.Features.Indexed;
using Swartz.Data;
using Swartz.Environment.Configuration;
using Swartz.Environment.ShellBuilders.Models;
using Swartz.Events;

namespace Swartz.Environment.ShellBuilders
{
    public interface IShellContainerFactory
    {
        ILifetimeScope CreateContainer(ShellSettings settings, ShellBlueprint blueprint);
    }

    public class ShellContainerFactory : IShellContainerFactory
    {
        private readonly ILifetimeScope _lifetimeScope;

        public ShellContainerFactory(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public ILifetimeScope CreateContainer(ShellSettings settings, ShellBlueprint blueprint)
        {
            var intermediateScope = _lifetimeScope.BeginLifetimeScope(builder =>
            {
                foreach (var item in blueprint.Dependencies.Where(t => typeof(IModule).IsAssignableFrom(t.Type)))
                {
                    builder.RegisterType(item.Type).Keyed<IModule>(item.Type).InstancePerDependency();
                }
            });

            return intermediateScope.BeginLifetimeScope("shell", builder =>
            {
                builder.Register(ctx => settings);
                builder.Register(ctx => blueprint);

                var moduleIndex = intermediateScope.Resolve<IIndex<Type, IModule>>();
                foreach (var item in blueprint.Dependencies.Where(t => typeof(IModule).IsAssignableFrom(t.Type)))
                {
                    builder.RegisterModule(moduleIndex[item.Type]);
                }

                foreach (var item in blueprint.Dependencies.Where(t => typeof(IDependency).IsAssignableFrom(t.Type)))
                {
                    var registration = builder.RegisterType(item.Type).InstancePerLifetimeScope();

                    foreach (
                        var interfaceType in
                            item.Type.GetInterfaces()
                                .Where(
                                    itf =>
                                        typeof(IDependency).IsAssignableFrom(itf) &&
                                        !typeof(IEventHandler).IsAssignableFrom(itf)))
                    {
                        registration = registration.As(interfaceType).AsSelf();
                        if (typeof(ISingletonDependency).IsAssignableFrom(interfaceType))
                        {
                            registration = registration.InstancePerMatchingLifetimeScope("shell");
                        }
                        else if (typeof(IUnitOfWorkDependency).IsAssignableFrom(interfaceType))
                        {
                            registration = registration.InstancePerMatchingLifetimeScope("work");
                        }
                        else if (typeof(ITransientDependency).IsAssignableFrom(interfaceType))
                        {
                            registration = registration.InstancePerDependency();
                        }
                    }

                    if (typeof(ITransactionManager).IsAssignableFrom(item.Type))
                    {
                        var interfaces = item.Type.GetInterfaces();
                        foreach (var interfaceType in interfaces)
                        {
                            if (interfaceType.GetInterface(typeof(ITransactionManager).Name) != null)
                            {
                                registration = registration.Named<ITransactionManager>(interfaceType.Name);
                            }
                        }
                    }

                    if (typeof(IEventHandler).IsAssignableFrom(item.Type))
                    {
                        var interfaces = item.Type.GetInterfaces();
                        foreach (var interfaceType in interfaces)
                        {
                            if (interfaceType.GetInterface(typeof(IEventHandler).Name) != null)
                            {
                                registration = registration.Named<IEventHandler>(interfaceType.Name);
                            }
                        }
                    }
                }

                foreach (var item in blueprint.Controllers)
                {
                    var serviceKeyName = (item.AreaName + "/" + item.ControllerName).ToLowerInvariant();
                    var serviceKeyType = item.Type;
                    builder.RegisterType(item.Type)
                        .Keyed<IController>(serviceKeyName)
                        .Keyed<IController>(serviceKeyType)
                        .WithMetadata("ControllerType", item.Type)
                        .InstancePerDependency();
                }

                foreach (var item in blueprint.HttpControllers)
                {
                    var serviceKeyName = (item.AreaName + "/" + item.ControllerName).ToLowerInvariant();
                    var serviceKeyType = item.Type;
                    builder.RegisterType(item.Type)
                        .Keyed<IHttpController>(serviceKeyName)
                        .Keyed<IHttpController>(serviceKeyType)
                        .WithMetadata("ControllerType", item.Type)
                        .InstancePerDependency();
                }
            });
        }
    }
}