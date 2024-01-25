
using System.Collections;
using System.Data.Common;
using Neo4j.Driver;
using Neo4jClient.Cypher;
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

        public async Task AddToHashSetFrom<T>(string key, Func<T, string> keySelector, List<T> list, TimeSpan expiry)
        {
            var db = _redis.GetDatabase();
            var hashEntries = list.Select(item => new HashEntry(keySelector(item), JsonConvert.SerializeObject(item)))
            .ToArray();
            await db.HashSetAsync(key, hashEntries);
            ExtendKey(key, expiry);
        }

        public async Task CreateHashSetFrom<T>(string setKey,T value, TimeSpan expiry)
        {
            var db = _redis.GetDatabase();
            var props = typeof(T).GetProperties();
            HashEntry[] entries = props.Select(prop => new HashEntry(prop.Name,JsonConvert.SerializeObject(prop.GetValue(value))))
            .ToArray();
            await db.HashSetAsync(setKey, entries);
            ExtendKey(setKey, expiry);
        }
        public async Task AddToHashSet<T>(string setKey, Func<T,string> keySelector,T value, TimeSpan expiry)
        {
            var db = _redis.GetDatabase();
            string serializedObject = JsonConvert.SerializeObject(value);
            await db.HashSetAsync(setKey, [new HashEntry(keySelector(value), serializedObject)]);
            ExtendKey(setKey, expiry);
        }

    
        public async Task<T> RemoveFromHashSet<T>(string key, string valueKey)
        {
            var db = _redis.GetDatabase();
            var ret = await GetFromHashSet<T>(key, valueKey);
            await db.HashDeleteAsync(key, valueKey);
            return ret;
        }

   
        public async Task<T> GetFromHashSet<T>(string setKey,string valueKey)
        {
            var db = _redis.GetDatabase();
            var value = db.HashGet(setKey,valueKey);

            return value.IsNullOrEmpty? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

       async public Task<List<T>>GetHashSetAsList<T>(string key)
       {    

            var db = _redis.GetDatabase();
            var list = new List<T>();
            var hashEntries = await db.HashGetAllAsync(key);
            foreach (var entry in hashEntries)
            {
                var value= JsonConvert.DeserializeObject<T>(entry.Value);
                list.Add(value!);
            }

            return list;
        }

        async public Task<T>GetHashSet<T>(string key) where T:new()
        {
            var db = _redis.GetDatabase();
            var value = new T();
            var hashEntries = await db.HashGetAllAsync(key);
            foreach (var entry in hashEntries)
            {
                var prop = entry.Name.ToString();
                var propValue = entry.Value.ToString();
                var actualProp = typeof(T).GetProperty(prop);
                actualProp?.SetValue(value, Convert.ChangeType(JsonConvert.DeserializeObject(propValue),
                actualProp.PropertyType));
            }

            return value;
        }

        public void IncrementHashField(string hashKey,string hashField,int value)
        {
            var db = _redis.GetDatabase();
            db.HashIncrement(hashKey, hashField,value);
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

        public async Task RemoveFromList<T>(string listKey, T value)
        {
            var db = _redis.GetDatabase();
            string jsonValue = JsonConvert.SerializeObject(value);
            await db.ListRemoveAsync(listKey, jsonValue, 1);
        }

        public async Task AddToOrderedHashFrom<T>(string key, Func<T,double> keySelector,List<T> list)
        {
            var db = _redis.GetDatabase();
            var sortedSetEntries = list.Select(item =>
            {
                return new SortedSetEntry(JsonConvert.SerializeObject(item), keySelector(item));
            });
            await db.SortedSetAddAsync(key, sortedSetEntries.ToArray());
        }

        public async Task RemoveFromOrderedHash(string key,string member)
        {
            var db = _redis.GetDatabase();
            await db.SortedSetRemoveAsync(key, JsonConvert.SerializeObject(member));
        }

    
        public async Task AddToOrderedHash<T>( string key,Func<T,double> keySelector,T val,TimeSpan expiry)
        {
            var db = _redis.GetDatabase();
            string serializedVal = JsonConvert.SerializeObject(val);
            await db.SortedSetAddAsync(key, serializedVal, keySelector(val));
            ExtendKey(key,expiry);
        }


        public async Task<T> DeleteFromOrderedHash<T>(string key, T value)
        {
            var db = _redis.GetDatabase();
            await db.SortedSetRemoveAsync(key,JsonConvert.SerializeObject(value));
            return value;
        }

    
        public async Task<List<T>> GetOrderedHash<T>(string key,Order order=Order.Descending)
        {
            var db = _redis.GetDatabase();
            var sortedSetEntries = await db.SortedSetRangeByScoreWithScoresAsync(key,order:order);
            var userSet = new List<T>();
            return sortedSetEntries != null ? sortedSetEntries.
            Select(item => JsonConvert.DeserializeObject<T>(item.Element!)!).ToList() : [];

        }

        public async Task UpdateHashSet(string hashKey, HashEntry[] fields,TimeSpan expiry)
        {
            var db = _redis.GetDatabase();
            await db.HashSetAsync(hashKey, fields);
            ExtendKey(hashKey, expiry);
        }
    }
}
