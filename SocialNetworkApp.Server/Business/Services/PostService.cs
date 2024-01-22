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
        string postId=await _repo.AddPost(post,userId);

        var friends = await _userService.GetFriends(userId);
        /*  var cacheKey = $"feed:{userId}"; */
        var cacheKey = $"feed:{userId}";
        if(_cacheService.KeyExists(cacheKey))
        {
            var postDto = await _repo.GetPost(postId, userId);
            await _cacheService.AddToListHeadAsync(cacheKey, postDto, TimeSpan.FromMinutes(2));
            await _cacheService.RemoveCacheValueAsync($"posts:{userId}++{userId}");
        }
        foreach (var friend in friends)
        {
            
            cacheKey = $"feed:{friend.UserId}";
            var cacheKeyPosts = $"posts:{userId}++{friend.UserId}";
            if(_cacheService.KeyExists(cacheKey))
            {
                var postDto = await _repo.GetPost(postId,friend.UserId);
                await _cacheService.AddToListHeadAsync(cacheKey, postDto, TimeSpan.FromMinutes(2));
                await _cacheService.RemoveCacheValueAsync(cacheKeyPosts);
            }
        }
    }


    public async Task<List<PostDTO>> GetPosts(string userId, int count, int skip,string currentUserId)
    {

        var cacheKey = $"posts:{userId}++{currentUserId}";
        var cachedPosts = await _cacheService.GetCacheValueAsync<List<PostDTO>>(cacheKey);
        if (cachedPosts != null)
        {
            
            return cachedPosts;
        }

        var posts = await _repo.GetPosts(userId,count,skip,currentUserId);
        await _cacheService.SetCacheValueAsync(cacheKey, posts, TimeSpan.FromMinutes(5));
        return posts;
    }


    public async Task DeletePost(string postId)
    {
        await _repo.DeletePost(postId);

        /* await _cacheService.RemoveCacheValueAsync($"post:{postId}");
        await _cacheService.RemoveCacheValueAsync($"{postId}:likes");
        await _cacheService.RemoveCacheValueAsync($"comments:{postId}"); */
    }

    public async Task<List<PostDTO>> GetFeed(string userId, int count)
    {
        var cacheKey = $"feed:{userId}";
        var cachedFeed = await _cacheService.GetListAsync<PostDTO>(cacheKey);
        if (cachedFeed != null && cachedFeed.Any())
        {
            return cachedFeed.ToList();
        } 
        var feed = await _repo.GetFeed(userId, count);
        await _cacheService.AddToListFrom(cacheKey, feed, TimeSpan.FromMinutes(2));
        return feed;
    }

    public async Task LikePost(string userId, string postId)
    {
        bool state = await _repo.LikePost(userId, postId);
        var postDto = await _repo.GetPost(postId, userId);
        var cacheKey = $"feed:{userId}";
        var cachedFeed = await _cacheService.GetListAsync<PostDTO>(cacheKey);
        var cacheKeyPosts = $"posts:{postDto.PostedById}++{userId}";
        var cachePosts = await _cacheService.GetCacheValueAsync<List<PostDTO>>(cacheKeyPosts);
        cachedFeed = cachedFeed.Select(post =>
        {
            if (post.PostId != postId)
                return post;
            post.Liked = state;
            post.Likes += state ? 1 : -1;
            return post;
        });
        if(cachePosts!=null)
        {
            cachePosts = cachePosts.Select(post =>
            {
                if (post.PostId != postId)
                    return post;
                post.Liked = state;
                post.Likes += state ? 1 : -1;
                return post;
            }).ToList();
            await _cacheService.SetCacheValueAsync(cacheKeyPosts, cachePosts.ToList(), TimeSpan.FromMinutes(2));
        }
     
        await _cacheService.RemoveCacheValueAsync(cacheKey);
        await _cacheService.AddToListFrom(cacheKey,cachedFeed.ToList(),TimeSpan.FromMinutes(2));


    }

    public async Task<List<UserDTO>> GetLikes(string postId,string userId)
    {
        //kesira sve ljude koji su lajkovali do sad
/*         var cacheKey = $"{postId}:likes";
        
        var cachedLikes = await _cacheService.GetCacheValueAsync<List<UserDTO>>(cacheKey);
        if (cachedLikes != null)
        {
            return cachedLikes;
        } */

        var likes = await _repo.GetLikes(postId,userId);
/*         await _cacheService.SetCacheValueAsync(cacheKey, likes, TimeSpan.FromMinutes(10)); */
        return likes;
    }
    //COMMENT
    public async Task AddComment(Post comment, string userId, string postId)
    {
        await _repo.AddComment(comment, userId, postId);

        var commentsCacheKey = $"comments:{postId}";
        var cachedComments = await _cacheService.GetCacheValueAsync<List<PostDTO>>(commentsCacheKey);
        if (cachedComments != null)
        {
            cachedComments.Add(new PostDTO
            {
                Content = comment.Content,
                MediaURL = comment.MediaURL,
                PostedBy = comment.PostedBy,
                PostedByPic = comment.PostedByPic,
                Timestamp = comment.Timestamp,
                Likes = 0,
                Liked = false
            }); 
            await _cacheService.SetCacheValueAsync(commentsCacheKey, cachedComments, TimeSpan.FromMinutes(5));
        }

        await _cacheService.PublishAsync($"post:commented:{postId}", JsonConvert.SerializeObject(comment));
    }

    public async Task<List<PostDTO>> GetComments(string postId,string userId)
    {
        var cacheKey = $"comments:{postId}";
        var cachedComments = await _cacheService.GetCacheValueAsync<List<PostDTO>>(cacheKey);
        if (cachedComments != null)
        {
            return cachedComments;
        }

        var comments = await _repo.GetComments(postId,userId);
        await _cacheService.SetCacheValueAsync(cacheKey, comments, TimeSpan.FromMinutes(5));
        return comments;
    }

}