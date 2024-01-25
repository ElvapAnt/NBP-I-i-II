using System.Collections;
using System.Collections.Generic;
using System.Net;
using Neo4jClient.Cypher;
using SocialNetworkApp.Server.Data.Entities;
using StackExchange.Redis;

namespace SocialNetworkApp.Server.Business.Services.Redis
{
    public interface ICacheService
    {
        public Task<T?> GetCacheValueAsync<T>(string key);
        public bool KeyExists(string key);

        public Task CreateHashSetFrom<T>(string setKey, T value, TimeSpan expiry);
        public void ExtendKey(string key,TimeSpan timeSpan);

        public TimeSpan? GetKeyTime(string key);
        public Task SetCacheValueAsync<T>(string key, T value, TimeSpan? expiry = null);

        public Task AddToHashSet<T>(string setKey, Func<T,string> keySelector,T value, TimeSpan expiry);
        public Task<T> GetFromHashSet<T>(string setKey,string valueKey);
        public Task AddToHashSetFrom<T>(string key, Func<T,string> keySelector,List<T> list, TimeSpan expiry);
        public Task<T> RemoveFromHashSet<T>(string key, string valueKey);

        public Task<List<T>> GetHashSetAsList<T>(string key);

        public Task<T> GetHashSet<T>(string key) where T:new();
        public Task AddToListAsync<T>(string listKey, T value,TimeSpan? expiry=null);

        public Task AddToListFrom<T>(string key, List<T> list, TimeSpan expiry);

        public Task AddToListHeadAsync<T>(string listKey, T value,TimeSpan? expiry=null);

        public Task RemoveFromList<T>(string listKey, T value);
        public Task<IEnumerable<T>> GetListAsync<T>(string listKey) ;
        public Task RemoveCacheValueAsync(string key);
        public Task PublishAsync(string channel, string message);
        public void Subscribe(string channel, Action<RedisChannel, RedisValue> onMessage);
        public Task EnqueueMessageAsync(string queueKey, string message);
        public Task<string?> DequeueMessageAsync(string queueKey);
        public Task AddToOrderedHashFrom<T>(string key, Func<T, double> keySelector, List<T> list);

        public Task RemoveFromOrderedHash(string key, string member);
        public Task AddToOrderedHash<T>(string key, Func<T, double> keySelector, T val,TimeSpan expiry);

        public Task<T> DeleteFromOrderedHash<T>(string key, T value);
        public Task<List<T>> GetOrderedHash<T>(string key, Order order=Order.Descending);
        public Task UpdateHashSet(string hashKey, HashEntry[] fields,TimeSpan expiry);

        public void IncrementHashField(string hashKey, string fieldName,int value);
    }

}
