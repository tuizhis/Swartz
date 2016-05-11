﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Features.Indexed;
using Swartz.Exceptions;

namespace Swartz.Events
{
    public class DefaultSwartzEventBus : IEventBus
    {
        private static readonly
            ConcurrentDictionary<string, Tuple<ParameterInfo[], Func<IEventHandler, object[], object>>> DelegateCache =
                new ConcurrentDictionary<string, Tuple<ParameterInfo[], Func<IEventHandler, object[], object>>>();

        private readonly IIndex<string, IEnumerable<IEventHandler>> _eventHandlers;

        public DefaultSwartzEventBus(IIndex<string, IEnumerable<IEventHandler>> eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public IEnumerable Notify(string messageName, IDictionary<string, object> eventData)
        {
            // call ToArray to ensure evaluation has taken place
            return NotifyHandlers(messageName, eventData).ToArray();
        }

        private IEnumerable<object> NotifyHandlers(string messageName, IDictionary<string, object> eventData)
        {
            var parameters = messageName.Split('.');
            if (parameters.Length != 2)
            {
                throw new ArgumentException($"{messageName} is not formatted correctly");
            }
            var interfaceName = parameters[0];
            var methodName = parameters[1];

            var eventHandlers = _eventHandlers[interfaceName];
            foreach (var eventHandler in eventHandlers)
            {
                IEnumerable returnValue;
                if (TryNotifyHandler(eventHandler, messageName, interfaceName, methodName, eventData, out returnValue))
                {
                    if (returnValue != null)
                    {
                        foreach (var value in returnValue)
                        {
                            yield return value;
                        }
                    }
                }
            }
        }

        private bool TryNotifyHandler(IEventHandler eventHandler, string messageName, string interfaceName,
            string methodName, IDictionary<string, object> eventData, out IEnumerable returnValue)
        {
            try
            {
                return TryInvoke(eventHandler, messageName, interfaceName, methodName, eventData, out returnValue);
            }
            catch (Exception exception)
            {
                if (exception.IsFatal())
                {
                    throw;
                }

                returnValue = null;
                return false;
            }
        }

        private static bool TryInvoke(IEventHandler eventHandler, string messageName, string interfaceName,
            string methodName, IDictionary<string, object> arguments, out IEnumerable returnValue)
        {
            var matchingInterface = eventHandler.GetType().GetInterface(interfaceName);
            return TryInvokeMethod(eventHandler, matchingInterface, messageName, interfaceName, methodName, arguments,
                out returnValue);
        }

        private static bool TryInvokeMethod(IEventHandler eventHandler, Type interfaceType, string messageName,
            string interfaceName, string methodName, IDictionary<string, object> arguments, out IEnumerable returnValue)
        {
            var key = eventHandler.GetType().FullName + "_" + messageName + "_" + string.Join("_", arguments.Keys);
            var cachedDelegate = DelegateCache.GetOrAdd(key, k =>
            {
                var method = GetMatchingMethod(eventHandler, interfaceType, methodName, arguments);
                return method != null
                    ? Tuple.Create(method.GetParameters(),
                        DelegateHelper.CreateDelegate<IEventHandler>(eventHandler.GetType(), method))
                    : null;
            });

            if (cachedDelegate != null)
            {
                var args = cachedDelegate.Item1.Select(methodParameter => arguments[methodParameter.Name]).ToArray();
                var result = cachedDelegate.Item2(eventHandler, args);

                returnValue = result as IEnumerable;
                if (result != null && (returnValue == null || result is string))
                    returnValue = new[] {result};
                return true;
            }

            returnValue = null;
            return false;
        }

        private static MethodInfo GetMatchingMethod(IEventHandler eventHandler, Type interfaceType, string methodName,
            IDictionary<string, object> arguments)
        {
            var allMethods = new List<MethodInfo>(interfaceType.GetMethods());
            var candidates = new List<MethodInfo>(allMethods);

            foreach (var method in allMethods)
            {
                if (string.Equals(method.Name, methodName, StringComparison.OrdinalIgnoreCase))
                {
                    var parameterInfos = method.GetParameters();
                    foreach (var parameter in parameterInfos)
                    {
                        if (!arguments.ContainsKey(parameter.Name))
                        {
                            candidates.Remove(method);
                            break;
                        }
                    }
                }
                else
                {
                    candidates.Remove(method);
                }
            }

            // treating common case separately
            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            if (candidates.Count != 0)
            {
                return candidates.OrderBy(x => x.GetParameters().Length).Last();
            }

            return null;
        }
    }
}