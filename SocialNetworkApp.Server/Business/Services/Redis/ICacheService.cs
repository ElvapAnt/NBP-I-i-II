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

        public void ExtendKey(string key,TimeSpan timeSpan);

        public TimeSpan? GetKeyTime(string key);
        public Task SetCacheValueAsync<T>(string key, T value, TimeSpan? expiry = null);
        public Task AddToListAsync<T>(string listKey, T value,TimeSpan? expiry=null) ;

        public Task AddToListFrom<T>(string key, List<T> list, TimeSpan expiry);

        public Task AddToListHeadAsync<T>(string listKey, T value,TimeSpan? expiry=null);
        public Task<IEnumerable<T>> GetListAsync<T>(string listKey) ;
        public Task RemoveCacheValueAsync(string key);
        public Task PublishAsync(string channel, string message);
        public void Subscribe(string channel, Action<RedisChannel, RedisValue> onMessage);
        public Task EnqueueMessageAsync(string queueKey, string message);
        public Task<string?> DequeueMessageAsync(string queueKey);
    }

    public class List
    {
    }
}
