using Newtonsoft.Json;
using SocialNetworkApp.Server.Business.Repos;
using SocialNetworkApp.Server.Business.Services.Redis;
using SocialNetworkApp.Server.Data.Entities;

namespace SocialNetworkApp.Server.Business.Services;

public class CacheInt(int val)
{
    public int Value { get; set; } = val;
}
public class PostService(PostRepo repo, ICacheService cacheService, UserService userService)
{
    private readonly PostRepo _repo = repo;
    private readonly ICacheService _cacheService = cacheService;
    private readonly UserService _userService = userService;
    
    public async Task AddPost(Post post,string userId)
    {
        await _repo.AddPost(post,userId);

        var postAddedMessage = JsonConvert.SerializeObject(new 
        {
            PostId = post.PostId,
            UserId = userId
        });
        await _cacheService.PublishAsync("posts:added", postAddedMessage);

        //za testiranje samo 10 prijatelja 
        //invalidira prethodno kesiran feed da ne bi uzeo stari bez novog posta
        //mada svakako se invalidira kroz 5 minuta, ali ajde
        var friends = await _userService.GetFriends(userId,10,0);
        foreach (var friend in friends)
        {
            var cacheKey = $"feed:{friend.UserId}";
            await _cacheService.RemoveCacheValueAsync(cacheKey);
        }

    }

    public async Task<List<Post>> GetPosts(string userId, int count, int skip)
    {
        return await _repo.GetPosts(userId,count,skip);
    }

    public async Task DeletePost(string postId)
    {
        await _repo.DeletePost(postId);

        await _cacheService.RemoveCacheValueAsync($"post:{postId}");
        await _cacheService.RemoveCacheValueAsync($"post:likes:{postId}");
        await _cacheService.RemoveCacheValueAsync($"post:comments:{postId}");
    }

    public async Task<List<Post>> GetFeed(string userId, int count)
    {
        var cacheKey = $"feed:{userId}";
        var cachedFeed = await _cacheService.GetCacheValueAsync<List<Post>>(cacheKey);
        if (cachedFeed != null)
        {
            return cachedFeed;
        } 
        var feed = await _repo.GetFeed(userId, count);
        await _cacheService.SetCacheValueAsync(cacheKey, feed, TimeSpan.FromMinutes(5));
        return feed;
    }

    public async Task LikePost(string userId,string postId)
    {
        await _repo.LikePost(userId, postId);

        //invalidira se kesirana lista lajkova
        var likesListCacheKey = $"post:likes:{postId}";
        await _cacheService.RemoveCacheValueAsync(likesListCacheKey);

        var likesCacheKey = $"likes:count:{postId}";
        var cachedLikes = await _cacheService.GetCacheValueAsync<CacheInt>(likesCacheKey);
        var newLikeCount = cachedLikes != null ? cachedLikes.Value + 1 : 1; // Assuming new like is added
        await _cacheService.SetCacheValueAsync(likesCacheKey, new CacheInt(newLikeCount), TimeSpan.FromMinutes(30));
        
        // Publish like event
        await _cacheService.PublishAsync($"post:liked:{postId}", userId);
    }

    public async Task<List<User>> GetLikes(string postId)
    {
        //kesira sve ljude koji su lajkovali do sad
        var cacheKey = $"post:likes:{postId}";
        
        var cachedLikes = await _cacheService.GetCacheValueAsync<List<User>>(cacheKey);
        if (cachedLikes != null)
        {
            return cachedLikes;
        }

        var likes = await _repo.GetLikes(postId);
        await _cacheService.SetCacheValueAsync(cacheKey, likes, TimeSpan.FromMinutes(30));
        return likes;
    }
    //COMMENT
    public async Task AddComment(Post comment, string userId, string postId)
    {
        await _repo.AddComment(comment, userId, postId);

        var commentsCacheKey = $"post:comments:{postId}";
        await _cacheService.RemoveCacheValueAsync(commentsCacheKey);

        await _cacheService.PublishAsync($"post:commented:{postId}", JsonConvert.SerializeObject(comment));
    }

    public async Task<List<Post>> GetComments(string postId)
    {
        var cacheKey = $"post:comments:{postId}";
        var cachedComments = await _cacheService.GetCacheValueAsync<List<Post>>(cacheKey);
        if (cachedComments != null)
        {
            return cachedComments;
        }

        var comments = await _repo.GetComments(postId);
        await _cacheService.SetCacheValueAsync(cacheKey, comments, TimeSpan.FromMinutes(30));
        return comments;
    }

}