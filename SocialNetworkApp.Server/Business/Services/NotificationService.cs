using SocialNetworkApp.Server.Business.Repos;
using SocialNetworkApp.Server.Data.Entities;

namespace SocialNetworkApp.Server.Business.Services;

public class NotificationService(NotificationRepo repo)
{
    private readonly NotificationRepo _repo = repo;

    public async Task AddRequest(Notification notification,string fromId,string toId)
    {
        await _repo.AddRequest(notification, fromId, toId);
    }

    public async Task DeleteRequest(string requestId)
    {
        await _repo.DeleteRequest(requestId);
    }

    public async Task<List<Notification>> GetReceivedRequests(string userId,int count,int skip)
    {
        return await _repo.GetReceivedRequests(userId, count, skip);
    }
}