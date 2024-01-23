using Neo4j.Driver;
using NRedisStack.Graph;
using SocialNetworkApp.Server.Data;
using SocialNetworkApp.Server.Data.Entities;

namespace SocialNetworkApp.Server.Business.Repos;

public class PostRepo(IDriver driver)
{
    private IDriver _driver = driver;

    public async Task<string> AddPost(Post post,string userId)
    {
        post.PostId = "post:"+Guid.NewGuid().ToString();
        post.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        using var session = _driver.AsyncSession();
        string query = "CREATE (p:Post $post) WITH p "+
        "MATCH (u:User{UserId:$userId}) MERGE (u)-[:POSTED]->(p) SET p.PostedBy=u.Username SET p.PostedByPic=u.Thumbnail SET p.PostedById=u.UserId"+
        " RETURN u";
        var parameters = new { post ,userId};
        await session.RunAsync(query, parameters);
        return post.PostId;
    }

    public async Task<List<PostDTO>> GetPosts(string userId,int count, int skip,string currentUserId)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (user:User{UserId:$currentUserId}), (u:User{UserId:$userId})-[:POSTED]->(p:Post) "+
        "WHERE NOT p:Comment  with user,u,p RETURN p {.*, Liked: EXISTS ((user)-[:LIKES]->(p))} ORDER BY p.Timestamp DESC SKIP $skip LIMIT $count";
        var parameters = new { currentUserId,userId,skip,count };
        var result = await session.RunAsync(query, parameters);
        var list = await result.ToListAsync();
        return RecordMapper.ToPostList(list, "p");
    }

    /// <summary>
    /// DO NOT CALL IF NOT SURE IF POST WITH POSTID EXISTS
    /// </summary>
    /// <param name="postId"></param>
    /// <param name="currentUserId"></param>
    /// <returns></returns>
    public async Task<PostDTO> GetPost(string postId,string currentUserId)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (post:Post{PostId:$postId}), (u:User{UserId:$currentUserId}) " +
        "RETURN post {.*,Liked: EXISTS ((u)-[:LIKES]->(post))}";
        var parameters = new { postId, currentUserId };
        var result = await session.RunAsync(query,parameters);
        await result.FetchAsync();
        var record = result.Current;
        return RecordMapper.ToPost(record, "post");
    }

    public async Task DeletePost(string postId)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (p:Post{PostId:$postId}) DETACH DELETE p";
        var parameters = new { postId };
        var result = await session.RunAsync(query, parameters);
    }

    public async Task<List<PostDTO>> GetFeed(string userId,int count)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (u:User{UserId:$userId})-[:FRIENDS]->(friend:User)-[:POSTED]->(p:Post) WHERE " +
        "NOT p:Comment AND NOT (u)-[:SEEN]->(p) WITH p,u ORDER BY p.Timestamp DESC LIMIT $count CREATE (u)-[:SEEN]->(p) "+
        "RETURN p {.*, Liked: EXISTS ((u)-[:LIKES]->(p))}";
        var parameters = new { userId, count };
        var result = await session.RunAsync(query, parameters);
        List<IRecord> list = await result.ToListAsync();
        return RecordMapper.ToPostList(list!, "p");
    }

    public async Task<Tuple<bool,UserDTO>>LikePost(string userId,string postId)
    {
        
        using var session = _driver.AsyncSession();
        string query0 = "MATCH (u:User{UserId:$userId}) "+
        "OPTIONAL MATCH (u)-[l:LIKES]->(p:Post{PostId:$postId}) return l is NOT NULL as value";
        var params0 = new { userId, postId };
        var result = await session.RunAsync(query0, params0);
        var record = await result.SingleAsync();
        bool value = record["value"].As<bool>();

        int step = !value ? 1 : -1;
        string query = "MATCH (u:User{UserId:$userId}), (p:Post{PostId:$postId}) WITH p,u " +
        (!value ? "MERGE (u)-[:LIKES]->(p)":"MATCH (u)-[l:LIKES]->(p) DELETE l")+" SET p.Likes = p.Likes + $step "+
        "RETURN u {.*, IsFriend:false,SentRequest:false,RecievedRequest:false}";
        var parameters = new { userId, postId, step };
        var res =await session.RunAsync(query, parameters);
        await res.FetchAsync();
        var userDTO = RecordMapper.ToUserDTO(res.Current, "u");
        return Tuple.Create(!value,userDTO);

    }

    public async Task<List<UserDTO>> GetLikes(string postId,string userId)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (u:User)-[l:LIKES]->(p:Post{PostId:$postId}), (u2:User{UserId:$userId}) "+
        "RETURN u {.*,IsFriend: EXISTS ((u)-[:FRIENDS]->(u2)), RecievedRequest: EXISTS ((u)-[:SENT]->(:Request)<-[:RECIEVED]-(u2)), "+
        "SentRequest: EXISTS ((u2)-[:SENT]->(:Request)<-[:RECIEVED]-(u))}";
        var parameters = new { postId,userId };
        var result = await session.RunAsync(query, parameters);
        var list = await result.ToListAsync();
        var parameters2 = new { postId,count=list.Count };
        query = "MATCH (p:Post{PostId:$postId}) SET p.Likes = $count";
        await session.RunAsync(query, parameters2);

        return RecordMapper.ToUserList(list, "u");
    }

    public async Task<PostDTO> AddComment(Post comment, string userId,string postId)
    {
        comment.PostId = "comment:"+Guid.NewGuid().ToString();
        comment.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        using var session = _driver.AsyncSession();
        string query = "CREATE (c:Comment:Post $comment) WITH c MATCH "+
        "(u:User{UserId:$userId}), (p:Post{PostId:$postId}), (c:Comment{PostId:$commentId}) CREATE (u)-[:POSTED]->(c) "+
        "CREATE (p)-[:HAS_COMMENT]->(c) SET c.PostedByPic=u.Thumbnail SET c.PostedBy = u.Username SET c.PostedById=u.UserId "+
        "RETURN c {.*, Liked: EXISTS((u)-[:LIKES]->(c))}";
        var parameters = new { comment,userId, postId,commentId = comment.PostId };
        var res=await session.RunAsync(query, parameters);
        await res.FetchAsync();
        return RecordMapper.ToPost(res.Current, "c");
    }

    public async Task<List<PostDTO>> GetComments(string postId,string userId)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (u:User{UserId:$userId}), (p:Post{PostId:$postId})-[:HAS_COMMENT]->(c:Comment) "+
        "RETURN c {.*, Liked: EXISTS ((u)-[:LIKES]->(c))} ORDER BY c.Likes DESC, "+
        "c.Timestamp DESC, c.PostId DESC";
        var parameters = new {userId, postId };
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