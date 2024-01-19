using Neo4j.Driver;
using SocialNetworkApp.Data;
using SocialNetworkApp.Server.Data.Entities;

namespace SocialNetworkApp.Server.Business.Repos;

public class PostRepo(IDriver driver)
{
    private IDriver _driver = driver;

    public async Task AddPost(Post post,string userId)
    {
        post.PostId = "post:"+Guid.NewGuid().ToString();
        post.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        using var session = _driver.AsyncSession();
        string query = "CREATE (p:Post $post) WITH p "+
        "MATCH (u:User{UserId:$userId}) MERGE (u)-[:POSTED]->(p) SET p.PostedBy=u.Username SET p.PostedByPic=u.Thumbnail "+
        "RETURN u";
        var parameters = new { post ,userId};
        await session.RunAsync(query, parameters);
    }

    public async Task<List<Post>> GetPosts(string userId,int count, int skip)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (u:User{UserId:$userId})-[:POSTED]->(p:Post) "+
        "WHERE NOT p:Comment RETURN p ORDER BY p.Timestamp DESC SKIP $skip LIMIT $count";
        var parameters = new { userId,skip,count };
        var result = await session.RunAsync(query, parameters);
        var list =await result.ToListAsync();
        return RecordMapper.ToPostList(list, "p");
    }

    public async Task DeletePost(string postId)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (p:Post{PostId:$postId}) DETACH DELETE p";
        var parameters = new { postId };
        var result = await session.RunAsync(query, parameters);
    }

    public async Task<List<Post>> GetFeed(string userId,int count)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (u:User{UserId:$userId})-[:FRIENDS]->(friend:User)-[:POSTED]->(p:Post) WHERE " +
        "NOT p:Comment AND NOT (u)-[:SEEN]->(p) WITH p,u ORDER BY p.Timestamp DESC LIMIT $count CREATE (u)-[:SEEN]->(p) RETURN p";
        var parameters = new { userId, count };
        var result = await session.RunAsync(query, parameters);
        List<IRecord> list = await result.ToListAsync();
        return RecordMapper.ToPostList(list!, "p");
    }

    public async Task LikePost(string userId,string postId)
    {
        
        using var session = _driver.AsyncSession();
        string query0 = "MATCH (u:User{UserId:$userId}) "+
        "OPTIONAL MATCH (u)-[l:LIKES]->(p:Post{PostId:$postId}) return l is NOT NULL as value";
        var params0 = new { userId, postId };
        var result = await session.RunAsync(query0, params0);
        var record = await result.SingleAsync();
        bool value =record["value"].As<bool>();

        int step = !value ? 1 : -1;
        string query = "MATCH (u:User{UserId:$userId}), (p:Post{PostId:$postId}) WITH p,u " +
        (!value ? "MERGE (u)-[:LIKES]->(p)":"MATCH (u)-[l:LIKES]->(p) DELETE l")+" SET p.Likes = p.Likes + $step";
        var parameters = new { userId, postId, step };
        await session.RunAsync(query, parameters);

    }

    public async Task<List<User>> GetLikes(string postId)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (u:User)-[l:LIKES]->(p:Post{PostId:$postId}) RETURN u";
        var parameters = new { postId };
        var result = await session.RunAsync(query, parameters);
        var list = await result.ToListAsync();
        var parameters2 = new { postId,count=list.Count };
        query = "MATCH (p:Post{PostId:$postId}) SET p.Likes = $count";
        await session.RunAsync(query, parameters2);

        return RecordMapper.ToUserList(list, "u");
    }

    public async Task AddComment(Post comment, string userId,string postId)
    {
        comment.PostId = "comment:"+Guid.NewGuid().ToString();
        comment.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        using var session = _driver.AsyncSession();
        string query = "CREATE (c:Comment:Post $comment) WITH c MATCH "+
        "(u:User{UserId:$userId}), (p:Post{PostId:$postId}), (c:Comment{PostId:$commentId}) CREATE (u)-[:COMMENTED]->(c) "+
        "CREATE (p)-[:HAS_COMMENT]->(c)";
        var parameters = new { comment,userId, postId,commentId = comment.PostId };
        await session.RunAsync(query, parameters);
    }

    public async Task<List<Post>> GetComments(string postId)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (p:Post{PostId:$postId})-[:HAS_COMMENT]->(c:Comment) return c ORDER BY c.Likes DESC, "+
        "c.Timestamp DESC, c.PostId DESC";
        var parameters = new { postId };
        var result =await session.RunAsync(query, parameters);
        var list = await result.ToListAsync();
        return RecordMapper.ToPostList(list, "c");
    }

    public async Task UpdatePost(Post post)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (p:Post{PostId:$postId}) SET p=$post";
        var parameters = new { postId = post.PostId, post };
        await session.RunAsync(query, parameters);
    }
}