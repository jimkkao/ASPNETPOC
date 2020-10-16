using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using StackExchange.Redis;

using SharedCacheContract;
using Newtonsoft.Json;

namespace RedisSharedCache
{
    public static class Helper
    {


        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect(GetRedisCacheName() + ",abortConnect=false,ssl=true,password=" + GetRedisPasswordName());
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        public static string GetRedisCacheName()
        {
            var cacheName = Environment.GetEnvironmentVariable("RedisCacheName");

            if( string.IsNullOrEmpty(cacheName))
            {
                cacheName = ConfigurationManager.AppSettings["RedisCacheName"];
            }

            return cacheName;
        }

        public static string GetRedisPasswordName()
        {
            var rcPwd = Environment.GetEnvironmentVariable("RedisCachePassword");

            if( string.IsNullOrEmpty(rcPwd) )
            {
                rcPwd = ConfigurationManager.AppSettings["RedisCachePassword"];
            }
            return rcPwd;
        }
    }

    public class SharedCache : ISharedCache
    {
        public SharedCache()
        {

        }

        public async Task<T> GetItemAsync<T>(string key)
        {
            var result = await Helper.Connection.GetDatabase().StringGetAsync(new RedisKey(key));

            if (result.HasValue)
            {
                return JsonConvert.DeserializeObject<T>(result.ToString());
            }

            return default(T);
        }


        public async Task SetItemAsync<T>(string key, T item, TimeSpan expired)
        {
            string strItem = JsonConvert.SerializeObject(item);

            await Helper.Connection.GetDatabase().StringSetAsync(new RedisKey(key),new RedisValue(strItem),expired, When.Always, CommandFlags.None);

        }


        public async Task SetItemAsync<T>(string key, T item)
        {
            string strItem = JsonConvert.SerializeObject(item);
            TimeSpan expired = TimeSpan.FromMinutes(60);

            await Helper.Connection.GetDatabase().StringSetAsync(new RedisKey(key), new RedisValue(strItem), expired, When.Always, CommandFlags.None);

        }
    }
}
