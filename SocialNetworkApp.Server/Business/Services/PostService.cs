using System.Runtime.CompilerServices;
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
        }
        await _cacheService.RemoveCacheValueAsync($"posts:{userId}++{userId}");
        foreach (var friend in friends)
        {
            
            cacheKey = $"feed:{friend.UserId}";
            var cacheKeyPosts = $"posts:{userId}++{friend.UserId}";
            if(_cacheService.KeyExists(cacheKey))
            {
                var postDto = await _repo.GetPost(postId,friend.UserId);
                await _cacheService.AddToListHeadAsync(cacheKey, postDto, TimeSpan.FromMinutes(2));
            }
            await _cacheService.RemoveCacheValueAsync(cacheKeyPosts);
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


    public async Task DeletePost(string postId,string userId)
    {
        await _repo.DeletePost(postId);
        var friends = await _userService.GetFriends(userId);
        var cacheKey = $"feed:{userId}";
        var cacheKeyPosts = $"posts:{userId}++{userId}";
        if(_cacheService.KeyExists(cacheKey))
        {
            
            var list = await _cacheService.GetListAsync<PostDTO>(cacheKey);
            TimeSpan? remainingTime = _cacheService.GetKeyTime(cacheKey);
            await _cacheService.RemoveCacheValueAsync(cacheKey);
            list = list.Where(item => item.PostId != postId);
            await _cacheService.AddToListFrom(cacheKey, list.ToList(), remainingTime??TimeSpan.FromMinutes(2));

        }
        if(_cacheService.KeyExists(cacheKeyPosts))
        {
            await _cacheService.RemoveCacheValueAsync(cacheKeyPosts);
        }
        foreach (var friend in friends)
        {
            
            cacheKey = $"feed:{friend.UserId}";
            cacheKeyPosts = $"posts:{userId}++{friend.UserId}";
            if(_cacheService.KeyExists(cacheKey))
            {
                var list = await _cacheService.GetListAsync<PostDTO>(cacheKey);
                TimeSpan? remainingTime = _cacheService.GetKeyTime(cacheKey);
                await _cacheService.RemoveCacheValueAsync(cacheKey);
                list = list.Where(item => item.PostId != postId);
                await _cacheService.AddToListFrom(cacheKey, list.ToList(), remainingTime??TimeSpan.FromMinutes(2));
            }
            if(_cacheService.KeyExists(cacheKeyPosts))
            {
                await _cacheService.RemoveCacheValueAsync(cacheKeyPosts);
            }
        }
        

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
        var (state,user)= await _repo.LikePost(userId, postId);
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
        var cacheKeyLikes = $"{postId}:likes";
        if(state && _cacheService.KeyExists(cacheKeyLikes))
        {
            await _cacheService.AddToListHeadAsync(cacheKeyLikes,user,TimeSpan.FromMinutes(2));
        }
        else if(!state && _cacheService.KeyExists(cacheKeyLikes))
        {
            var list = await _cacheService.GetListAsync<UserDTO>(cacheKeyLikes);
            list = list.Where(item => item.UserId != userId);
            await _cacheService.RemoveCacheValueAsync(cacheKeyLikes);
            await _cacheService.AddToListFrom(cacheKeyLikes,list.ToList(),TimeSpan.FromMinutes(2));
        }
        await _cacheService.RemoveCacheValueAsync(cacheKey);
        await _cacheService.AddToListFrom(cacheKey,cachedFeed.ToList(),TimeSpan.FromMinutes(2));
    
    }

    public async Task LikeComment(string commentId,string userId,string postId)
    {
        var (state,user) =await _repo.LikePost(userId, commentId);
        var cacheKey = $"comments:{postId}++{userId}";
        var cacheKeyLikes = $"{commentId}:likes";
        var cachedComments = await _cacheService.GetListAsync<PostDTO>(cacheKey);
        cachedComments = cachedComments.Select<PostDTO,PostDTO>(c =>
        {
            if (c.PostId == commentId)
            {
                c.Liked = state;
                c.Likes += state ? 1 : -1;
            }
            return c;

        });
         if(state && _cacheService.KeyExists(cacheKeyLikes))
        {
            await _cacheService.AddToListHeadAsync(cacheKeyLikes,user,TimeSpan.FromMinutes(2));
        }
        else if(!state && _cacheService.KeyExists(cacheKeyLikes))
        {
            var list = await _cacheService.GetListAsync<UserDTO>(cacheKeyLikes);
            list = list.Where(item => item.UserId != userId);
            await _cacheService.RemoveCacheValueAsync(cacheKeyLikes);
            await _cacheService.AddToListFrom(cacheKeyLikes,list.ToList(),TimeSpan.FromMinutes(2));
        }
        await _cacheService.RemoveCacheValueAsync(cacheKey);
        await _cacheService.AddToListFrom(cacheKey,cachedComments.ToList(),TimeSpan.FromMinutes(2));
    }

    public async Task DeleteComment(string commentId,string userId,string postId)
    {

        await _repo.DeletePost(commentId);
    }
    
    public async Task<List<UserDTO>> GetLikes(string postId,string userId)
    {
       
        var cacheKey = $"{postId}:likes";
        var cachedLikes = await _cacheService.GetListAsync<UserDTO>(cacheKey);
        if (cachedLikes != null && cachedLikes.Any())
        {
            return cachedLikes.ToList();
        } 

        var likes = await _repo.GetLikes(postId,userId);
        await _cacheService.AddToListFrom(cacheKey, likes, TimeSpan.FromMinutes(2));
        return likes;
    }
    //COMMENT
    public async Task AddComment(Post comment, string userId, string postId)
    {
        var commentDTO=await _repo.AddComment(comment, userId, postId);

        var commentsCacheKey = $"comments:{postId}++{userId}";
        if (_cacheService.KeyExists(commentsCacheKey))
        {
            await _cacheService.AddToListHeadAsync(commentsCacheKey, commentDTO, TimeSpan.FromMinutes(5));
        }

        await _cacheService.PublishAsync($"post:commented:{postId}", JsonConvert.SerializeObject(comment));
    }

    public async Task<List<PostDTO>> GetComments(string postId,string userId)
    {
        var cacheKey = $"comments:{postId}++{userId}";
        var cachedComments = await _cacheService.GetListAsync<PostDTO>(cacheKey);
        if (cachedComments != null && cachedComments.Any())
        {
            return cachedComments.ToList();
        }

        var comments = await _repo.GetComments(postId,userId);
        await _cacheService.AddToListFrom(cacheKey, comments, TimeSpan.FromMinutes(5));
        return comments;
    }

}