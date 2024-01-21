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

        //za testiranje samo 10 prijatelja 
        //invalidira prethodno kesiran feed da ne bi uzeo stari bez novog posta
        //mada svakako se invalidira kroz 5 minuta, ali ajde
        var friends = await _userService.GetFriends(userId,10,0);
        foreach (var friend in friends)
        {
            //sacuva se u kes za profil
            var postCacheKey = $"posts:{userId}";
            var cacheKey = $"feed:{friend.UserId}";
            var postCache = await _cacheService.GetCacheValueAsync<List<PostDTO>>(postCacheKey);
            var cachedFeed = await _cacheService.GetCacheValueAsync<List<PostDTO>>(cacheKey);
            if (cachedFeed != null)
            {
                var p = new PostDTO
                {
                    PostId = post.PostId,
                    Content = post.Content,
                    MediaURL = post.MediaURL,
                    PostedBy = post.PostedBy,
                    PostedByPic = post.PostedByPic,
                    Timestamp = post.Timestamp,
                    Likes = 0,
                    Liked = false
                };
                cachedFeed.Add(p);
                if(postCache!=null)
                    postCache.Add(p);
                await _cacheService.SetCacheValueAsync(cacheKey, cachedFeed, TimeSpan.FromMinutes(5));
                await _cacheService.SetCacheValueAsync(postCacheKey, cachedFeed, TimeSpan.FromMinutes(10));
            }
        }

        await _cacheService.PublishAsync("posts:added", postAddedMessage);

    }

    public async Task<List<PostDTO>> GetPosts(string userId, int count, int skip,string currentUserId)
    {

        //provera da li postoji u kesu za brze prikupljanje
        var cacheKey = $"posts:{userId}";
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

        await _cacheService.RemoveCacheValueAsync($"post:{postId}");
        await _cacheService.RemoveCacheValueAsync($"{postId}:likes");
        await _cacheService.RemoveCacheValueAsync($"comments:{postId}");
    }

    public async Task<List<PostDTO>> GetFeed(string userId, int count)
    {
        //dobije se feed jednom pa se 
        var cacheKey = $"feed:{userId}";
        var cachedFeed = await _cacheService.GetCacheValueAsync<List<PostDTO>>(cacheKey);
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

        var cacheKey = $"feed:{userId}";
        var likesListCacheKey = $"{postId}:likes";
        var likesCacheKey = $"likes:count:{postId}";

        // Get the current cached likes list and like count
        var postLikes = await _cacheService.GetCacheValueAsync<List<UserDTO>>(likesListCacheKey);
        var cachedLikes = await _cacheService.GetCacheValueAsync<CacheInt>(likesCacheKey);
        var currentUser = await _userService.GetUser(userId);

        if (postLikes != null && cachedLikes != null && currentUser != null)
        {

            bool hasLiked = postLikes?.Any(u => u.UserId == userId) ?? false;

            if (hasLiked)
            {
                postLikes!.Remove(postLikes.First(u => u.UserId == userId));
                cachedLikes!.Value -= 1;
            }
            else
            {
                postLikes!.Add(currentUser); // Assuming UserDTO structure
                cachedLikes.Value += 1;
            }

            await _cacheService.SetCacheValueAsync(likesListCacheKey, postLikes, TimeSpan.FromMinutes(10));
            await _cacheService.SetCacheValueAsync(likesCacheKey, new CacheInt(cachedLikes.Value), TimeSpan.FromMinutes(10));

            // Invalidate user feed cache
            await _cacheService.RemoveCacheValueAsync(cacheKey);
        }

        // Publish like event
        await _cacheService.PublishAsync($"post:liked:{postId}", userId);
        await _repo.LikePost(userId, postId);
    }

    public async Task<List<UserDTO>> GetLikes(string postId,string userId)
    {
        //kesira sve ljude koji su lajkovali do sad
        var cacheKey = $"{postId}:likes";
        
        var cachedLikes = await _cacheService.GetCacheValueAsync<List<UserDTO>>(cacheKey);
        if (cachedLikes != null)
        {
            return cachedLikes;
        }

        var likes = await _repo.GetLikes(postId,userId);
        await _cacheService.SetCacheValueAsync(cacheKey, likes, TimeSpan.FromMinutes(10));
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