using SocialNetworkApp.Server.Business.Repos;
using SocialNetworkApp.Server.Business.Services.Redis;
using SocialNetworkApp.Server.Data.Entities;

namespace SocialNetworkApp.Server.Business.Services;

public class NotificationService(NotificationRepo repo, ICacheService cacheService)
{
    private readonly NotificationRepo _repo = repo;
    private readonly ICacheService _cacheService = cacheService;

    public async Task AddRequest(Notification notification,string fromId,string toId)
    {
        //invalidiramo kes za notifik onom kom saljemo

        await _repo.AddRequest(notification, fromId, toId);


        var cacheKey = $"notifications:received:{toId}";
        if(_cacheService.KeyExists(cacheKey))
            await _cacheService.AddToListHeadAsync(cacheKey, notification, TimeSpan.FromMinutes(2));

       /*  var cacheUserKey = $"{fromId}";
        var cachedUser = await _cacheService.GetCacheValueAsync<UserDTO>(cacheUserKey);
        cachedUser!.SentRequest = true;
        await _cacheService.SetCacheValueAsync(cacheUserKey, cachedUser, TimeSpan.FromMinutes(30));

        cacheUserKey = $"{toId}";
        cachedUser = await _cacheService.GetCacheValueAsync<UserDTO>(cacheUserKey);
        cachedUser!.RecievedRequest = true;
        await _cacheService.SetCacheValueAsync(cacheUserKey, cachedUser, TimeSpan.FromMinutes(30));  */
    }

    public async Task DeleteRequest(string requestId,string userId)
    {
        await _repo.DeleteRequest(requestId);
        await _cacheService.RemoveCacheValueAsync($"notifications:received:{userId}");
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