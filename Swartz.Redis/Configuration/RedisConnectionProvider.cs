using System;
using System.Collections.Concurrent;
using System.Configuration;
using StackExchange.Redis;
using Swartz.Environment.Configuration;
using Swartz.Logging;

namespace Swartz.Redis.Configuration
{
    public class RedisConnectionProvider : IRedisConnectionProvider
    {
        private static readonly ConcurrentDictionary<string, Lazy<ConnectionMultiplexer>> ConnectionMultiplexers =
            new ConcurrentDictionary<string, Lazy<ConnectionMultiplexer>>();

        private readonly ShellSettings _shellSettings;

        public RedisConnectionProvider(ShellSettings settings)
        {
            _shellSettings = settings;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ConnectionMultiplexer GetConnection(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            // when using ConcurrentDictionary, multiple threads can create the value
            // at the same time, so we need to pass a Lazy so that it's only 
            // the object which is added that will create a ConnectionMultiplexer,
            // even when a delegate is passed

            return
                ConnectionMultiplexers.GetOrAdd(connectionString,
                    new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connectionString))).Value;
        }

        public string GetConnectionString(string service)
        {
            var tenantSettingsKey = _shellSettings.Name + ":" + service;
            var defaultSettingsKey = service;

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[tenantSettingsKey] ??
                                           ConfigurationManager.ConnectionStrings[defaultSettingsKey];

            if (connectionStringSettings == null)
            {
                throw new ConfigurationErrorsException("A connection string is expected for " + service);
            }

            return connectionStringSettings.ConnectionString;
        }
    }
}