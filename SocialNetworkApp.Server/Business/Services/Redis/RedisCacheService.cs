
using System.Collections;
using NetTopologySuite.Index.HPRtree;
using Newtonsoft.Json;
using SocialNetworkApp.Server.Data.Entities;
using StackExchange.Redis;

namespace SocialNetworkApp.Server.Business.Services.Redis
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<T?> GetCacheValueAsync<T>(string key) 
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);
            return value.IsNullOrEmpty ? default(T) : JsonConvert.DeserializeObject<T>(value);
            
        }

        public TimeSpan? GetKeyTime(string key)
        {
            return _redis.GetDatabase().KeyTimeToLive(key);
        }

        public void ExtendKey(string key,TimeSpan timeSpan)
        {
            _redis.GetDatabase().KeyExpire(key,timeSpan);
        }

        public bool KeyExists(string key)
        {
            return _redis.GetDatabase().KeyExists(key);
        }
        public async Task SetCacheValueAsync<T>(string key, T value, TimeSpan? expiry = null) 
        {
            var db = _redis.GetDatabase();
            var serializedValue = JsonConvert.SerializeObject(value);
            await db.StringSetAsync(key, serializedValue, expiry);
        }

        public async Task AddToListAsync<T>(string listKey, T value,TimeSpan? expiry=null) 
        {
            var db = _redis.GetDatabase();
            var serializedValue = JsonConvert.SerializeObject(value);
            await db.ListRightPushAsync(listKey, serializedValue );
        }

        public async Task AddToListFrom<T>(string key, List<T> list, TimeSpan expiry) 
        {
            var db = _redis.GetDatabase();
            var cacheList = list.Select(item => JsonConvert.SerializeObject(item));
            foreach(var item in cacheList)
            {
                await db.ListRightPushAsync(key, item);
            }
            db.KeyExpire(key, expiry);
        }

        public async Task AddToListHeadAsync<T>(string listKey,T value,TimeSpan? expiry=null)
        {
            var db = _redis.GetDatabase();
            var serializedValue = JsonConvert.SerializeObject(value);
            await db.ListLeftPushAsync(listKey, serializedValue);
            if(expiry!=null)
            {
                db.KeyExpire(listKey, expiry);
            }
        }

        public async Task<IEnumerable<T>> GetListAsync<T>(string listKey) 
        {
            var db = _redis.GetDatabase();
            var values = await db.ListRangeAsync(listKey);
            return values.Select(value => JsonConvert.DeserializeObject<T>(value)).ToList();
        }

        public async Task RemoveCacheValueAsync(string key)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
        }

        public async Task PublishAsync(string channel, string message)
        {
            var subscriber = _redis.GetSubscriber();
            var redisChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);
            await subscriber.PublishAsync(redisChannel, message);
        }

        public void Subscribe(string channel, Action<RedisChannel, RedisValue> onMessage)
        {
            var subscriber = _redis.GetSubscriber();
            var redisChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);
            subscriber.Subscribe(redisChannel, (ch, value) => onMessage(ch,value));
        }

        public async Task EnqueueMessageAsync(string queueKey, string message)
        {
            var db = _redis.GetDatabase();
            await db.ListRightPushAsync(queueKey, message);
        }

        public async Task<string?> DequeueMessageAsync(string queueKey)
        {
            var db = _redis.GetDatabase();
            var value = await db.ListLeftPopAsync(queueKey);
            return value.HasValue ? value.ToString() : null;
        }
    }
}
