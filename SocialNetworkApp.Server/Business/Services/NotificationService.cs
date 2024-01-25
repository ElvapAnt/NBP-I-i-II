using Newtonsoft.Json;
using SocialNetworkApp.Server.Business.Repos;
using SocialNetworkApp.Server.Business.Services.Redis;
using SocialNetworkApp.Server.Data.Entities;
using StackExchange.Redis;

namespace SocialNetworkApp.Server.Business.Services;

public class NotificationService(NotificationRepo repo, ICacheService cacheService)
{
    private readonly NotificationRepo _repo = repo;
    private readonly ICacheService _cacheService = cacheService;

    public async Task AddRequest(Notification notification,string fromId,string toId)
    {

        notification = await _repo.AddRequest(notification, fromId, toId);
        var cacheKey = $"notifications:received:{toId}";
        if(_cacheService.KeyExists(cacheKey))
            await _cacheService.AddToListHeadAsync(cacheKey, notification, TimeSpan.FromMinutes(2));

        var cacheUserToUserKey = $"{toId}++{fromId}";
        await _cacheService.UpdateHashSet(cacheUserToUserKey,[
            new("SentRequest","true")
        ],TimeSpan.FromMinutes(2));
        
        cacheUserToUserKey = $"{fromId}++{toId}";
        if(_cacheService.KeyExists(cacheUserToUserKey))
        await _cacheService.UpdateHashSet(cacheUserToUserKey,[
            new("RecievedRequest","true")
        ],TimeSpan.FromMinutes(2));
    }

    public async Task DeleteRequest(string requestId,string userId)
    {
        Notification request = await _repo.DeleteRequest(requestId);
        await _cacheService.RemoveFromList($"notifications:received:{userId}", request);
    }

    public async Task<List<Notification>> GetReceivedRequests(string userId,int count,int skip)
    {
        var cacheKey = $"notifications:received:{userId}";
        var cachedNotifications = await _cacheService.GetListAsync<Notification>(cacheKey);
        if (cachedNotifications != null && cachedNotifications.Any())
            return cachedNotifications.ToList();

        var notifications = await _repo.GetReceivedRequests(userId, count, skip);
        await _cacheService.AddToListFrom(cacheKey, notifications, TimeSpan.FromMinutes(2));
        return notifications;
    }
}