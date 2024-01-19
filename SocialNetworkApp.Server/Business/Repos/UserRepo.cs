using Neo4j.Driver;
using SocialNetworkApp.Data;
using SocialNetworkApp.Server.Data.Entities;

namespace SocialNetworkApp.Server.Business.Repos;

public class UserRepo(IDriver driver)
{
    private readonly IDriver _driver = driver;

    public async Task AddUser(User user)
    {
        user.UserId = "user:"+Guid.NewGuid().ToString();
        using var session = _driver.AsyncSession();
        string query = "CREATE (u: User $user) RETURN u";
        var parameters = new { user };

        await session.RunAsync(query, parameters);
    }

    public async Task<User?> GetUser(string userId)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (u : User{UserId: $userId}) RETURN u";
        var parameters = new { userId };
        var result =await session.RunAsync(query, parameters);

        bool success = await result.FetchAsync();
        if(success)
        {
            var record = result.Current;

            return RecordMapper.ToUser(record,"u");
        }
        return null;
    }

    public async Task<User?> GetUserByUsername(string username)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (u:User WHERE u.Username = $username) RETURN u";
        var parameters = new { username };
        var result =await session.RunAsync(query, parameters);

        bool success = await result.FetchAsync();
        if(success)
        {
            var record = result.Current;

            return RecordMapper.ToUser(record,"u");
        }
        return null;

    }

    public async Task DeleteUser(string userId)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (u:User{UserId:$userId}) DELETE u";
        var parameters = new { userId };
        await session.RunAsync(query, parameters);
    }

    public async Task UpdateUser(User user)
    {
        using var session = _driver.AsyncSession();
        string query ="MATCH (u:User{UserId:$userId}) SET u=$user RETURN u";
        var parameters = new { userId = user.UserId, user };
        await session.RunAsync(query, parameters);
    }

    public async Task UpdateUsersPosts(string userId)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (u:User{UserId:$userId})-[:POSTED]->(p:Post) SET p.PostedByPic=u.Thumbnail SET p.PostedBy=u.Username";
        var parameters = new { userId };
        await session.RunAsync(query, parameters);
    }

    public async Task UpdateUsersChats(string userId)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (u:User{UserId:$userId})-[:MEMBER_OF]->(c:Chat) WITH c,u " +
        "SET c.Members = [member in c.Members | CASE WHEN member CONTAINS $userId THEN $userId+' '+u.Username+' '+u.Thumbnail " +
        "ELSE member END]";
        var parameters = new { userId };
        await session.RunAsync(query, parameters);
    }

    public async Task UpdateUsersNotifications(string userId)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (u:User{UserId:$userId})-[:SENT]->(n:Notification) WITH u,n " +
        "SET n.From = u.Username";
        var parameters = new { userId };
        await session.RunAsync(query, parameters);
    }

    public async Task AddFriend(string userId1,string userId2)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (u1:User{UserId:$userId1}), (u2:User{UserId:$userId2})" +
        "MERGE (u1)-[:FRIENDS]->(u2) MERGE (u2)-[:FRIENDS]->(u1)";
        var parameters = new { userId1, userId2 };
        await session.RunAsync(query, parameters);
    }

    public async Task<List<User>> GetFriends(string userId,int count=0x7FFFFFFF,int skip=0)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (u:User{UserId:$userId})-[:FRIENDS]->(friend:User) WITH friend as f Order By "+ 
        "f.Name,f.UserId RETURN f " +
        "SKIP $skip LIMIT $count";
        var parameters = new { userId, skip, count };
        var result = await session.RunAsync(query, parameters);
        List<User> friends = RecordMapper.ToUserList(await result.ToListAsync(), "f");

        return friends;
    }



    public async Task<List<User>> GetRecommendedFriends(string userId,int count,int skip)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (u:User{UserId:$userId})-[:FRIENDS]->(friend:User)-[:FRIENDS]->(recommended:User) " +
        "WHERE NOT (u)-[:FRIENDS]->(recommended) AND recommended<>u WITH recommended, "+
        "COUNT(DISTINCT friend) as mutualFriendCount "+
        "ORDER BY mutualFriendCount DESC, recommended.Name ASC, recommended.UserId ASC RETURN recommended SKIP $skip LIMIT $count";
        var parameters = new { userId, skip, count };
        var result = await session.RunAsync(query, parameters);

        return RecordMapper.ToUserList(await result.ToListAsync(), "recommended");

    }

    public async Task<List<User>> SearchForUsers(string usernamePattern)
    {
        using var session = _driver.AsyncSession();
        string lowercasePattern = usernamePattern.ToLower();
        var query = 
        "MATCH (u:User) WHERE toLower(u.Username) CONTAINS $lowercasePattern WITH u, "+
        "toLower(u.Username) as lowercaseUsername RETURN u "+
        "ORDER BY CASE "+
        "WHEN lowercaseUsername STARTS WITH $lowercasePattern THEN 1 "+
        "WHEN lowercaseUsername CONTAINS $lowercasePattern AND NOT lowercaseUsername ENDS WITH $lowercasePattern THEN 2 "+
        "ELSE 3 END, lowercaseUsername";
        var parameters = new { lowercasePattern};
        var result = await session.RunAsync(query, parameters);
        var list =await result.ToListAsync();
        return RecordMapper.ToUserList(list, "u");
    }
}