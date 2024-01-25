using Neo4j.Driver;
using NetTopologySuite.Operation.Buffer;
using NRedisStack.Graph;
using SocialNetworkApp.Server.Data;
using SocialNetworkApp.Server.Data.Entities;

namespace SocialNetworkApp.Server.Business.Repos;

public class NotificationRepo(IDriver driver)
{
    private readonly IDriver _driver = driver;

    public async Task<Notification> AddRequest(Notification notification,string fromId,string toId)
    {
        using var session = _driver.AsyncSession();
        notification.NotificationId = "request:" + Guid.NewGuid().ToString();
        notification.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string query = "CREATE (n:Notification:Request $notification) WITH n " +
        "MATCH (userFrom:User{UserId:$fromId}),(userTo:User{UserId:$toId}) MERGE " +
        "(userFrom)-[:SENT]->(n)<-[:RECIEVED]-(userTo) SET n.From=userFrom.Username SET n.Thumbnail=userFrom.Thumbnail "+
        "SET n.URL = userFrom.UserId RETURN n";
        var parameters = new { notification, fromId, toId };
        var res =await session.RunAsync(query, parameters);
        await res.FetchAsync();
        return RecordMapper.ToNotification(res.Current, "n");
    }

    public async Task<Notification> DeleteRequest(string requestId)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (r:Request{NotificationId:$requestId}) with r, PROPERTIES(r) as request DETACH DELETE r "+
        "RETURN request";
        var parameters = new { requestId };
        var res =await session.RunAsync(query, parameters);
        await res.FetchAsync();
        return RecordMapper.ToNotification(res.Current, "request");
    }

    public async Task<List<Notification>> GetReceivedRequests(string userId,int count,int skip)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (user:User{UserId:$userId})-[:RECIEVED]->(r:Request) SET r.Viewed=true RETURN r ORDER BY r.Timestamp DESC, "+
        "r.NotificationId DESC " +
        "SKIP $skip LIMIT $count";
        var parameters = new { userId, skip, count };
        var result = await session.RunAsync(query, parameters);
        var list = await result.ToListAsync();
        return RecordMapper.ToNotificationList(list, "r");
    }

    public async Task<List<Notification>> GetSentRequests(string userId)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (user:User{UserId:$userId})-[:SENT]->(r:Request) RETURN r ORDER BY r.Timestamp DESC, " +
        "r.NotificationId DESC";
        var parameters = new { userId};
        var result = await session.RunAsync(query, parameters);
        var list = await result.ToListAsync();
        return RecordMapper.ToNotificationList(list, "r");
    }
}