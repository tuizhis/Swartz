using System;
using StackExchange.Redis;
using Swartz.Caching;
using Swartz.Environment.Configuration;
using Swartz.Redis.Configuration;
using Swartz.Redis.Extensions;
using Swartz.Services;

namespace Swartz.Redis.Caching
{
    public class RedisCacheStorageProvider : Component, ICacheStorageProvider
    {
        public const string ConnectionStringKey = "Swartz.Redis.Cache";
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IJsonConverter _jsonConverter;

        private readonly ShellSettings _shellSettings;

        public RedisCacheStorageProvider(ShellSettings shellSettings, IRedisConnectionProvider redisConnectionProvider,
            IJsonConverter jsonConverter)
        {
            _shellSettings = shellSettings;
            var redisConnectionProvider1 = redisConnectionProvider;
            var connectionString = redisConnectionProvider1.GetConnectionString(ConnectionStringKey);
            _connectionMultiplexer = redisConnectionProvider1.GetConnection(connectionString);
            _jsonConverter = jsonConverter;
        }

        public IDatabase Database => _connectionMultiplexer.GetDatabase();

        public object Get<T>(string key)
        {
            var json = Database.StringGet(GetLocalizedKey(key));
            if (string.IsNullOrEmpty(json))
            {
                return default(T);
            }

            return _jsonConverter.Deserialize<T>(json);
        }

        public void Put<T>(string key, T value)
        {
            var json = _jsonConverter.Serialize(value);
            Database.StringSet(GetLocalizedKey(key), json);
        }

        public void Put<T>(string key, T value, TimeSpan validFor)
        {
            var json = _jsonConverter.Serialize(value);
            Database.StringSet(GetLocalizedKey(key), json, validFor);
        }

        public void Remove(string key)
        {
            Database.KeyDelete(GetLocalizedKey(key));
        }

        public void Clear()
        {
            Database.KeyDeleteWithPrefix(GetLocalizedKey("*"));
        }

        private string GetLocalizedKey(string key)
        {
            return _shellSettings.Name + ":Cache:" + key;
        }
    }
}