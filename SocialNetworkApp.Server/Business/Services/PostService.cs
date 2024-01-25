using SocialNetworkApp.Server.Business.Repos;
using SocialNetworkApp.Server.Business.Services.Redis;
using SocialNetworkApp.Server.Data.Entities;

namespace SocialNetworkApp.Server.Business.Services;
public class PostService(PostRepo repo, ICacheService cacheService, UserService userService)
{
    private readonly PostRepo _repo = repo;
    private readonly ICacheService _cacheService = cacheService;
    private readonly UserService _userService = userService;
    
    public async Task AddPost(Post post,string userId)
    {
        var postDto=await _repo.AddPost(post,userId);

        string postCacheKey = postDto.PostId;
        await _cacheService.CreateHashSetFrom<PostDTO>(postCacheKey, postDto, TimeSpan.FromMinutes(1));
        var friends = await _userService.GetFriends(userId);
        string cacheKeyPosts = $"{userId}:posts";
        if(_cacheService.KeyExists(cacheKeyPosts))
        {
            await _cacheService.AddToOrderedHash(cacheKeyPosts,x=>postDto.Timestamp,
             postDto.PostId,TimeSpan.FromMinutes(1));
        }
        foreach (var item in friends.Select(friend=>friend.UserId).Append(userId))
        {
            string cacheKey = $"{item}:feed";
            if (_cacheService.KeyExists(cacheKey))
            {
                await _cacheService.AddToOrderedHash(cacheKey, x=> (double)(postDto.Timestamp),
                postDto.PostId, TimeSpan.FromMinutes(1));
                await _cacheService.SetCacheValueAsync($"liked:{postCacheKey}+{item}", "false", TimeSpan.FromMinutes(1));
            }
        } 
    }


    public async Task<List<PostDTO>> GetPosts(string userId, int count, int skip,string currentUserId)
    {
        string postsCacheKey = $"{userId}:posts";
        TimeSpan delta = TimeSpan.FromSeconds(120);
        if(_cacheService.KeyExists(postsCacheKey))
        {
            _cacheService.ExtendKey(postsCacheKey,delta);
            List<string> postIds = await _cacheService.GetOrderedHash<string>(postsCacheKey);
            List<PostDTO> postDTOs = new(postIds.Count);
            foreach(string postId in postIds)
            {
                string likesCacheKey = $"liked:{postId}+{currentUserId}";
                if(_cacheService.KeyExists(postId) && _cacheService.KeyExists(likesCacheKey))
                {
                    var post = await _cacheService.GetHashSet<PostDTO>(postId);
                    post.Liked = await _cacheService.GetCacheValueAsync<bool>(likesCacheKey);
                    postDTOs.Add(post);
                    _cacheService.ExtendKey(postId, delta);
                    _cacheService.ExtendKey(likesCacheKey, delta);
                }
                else
                {
                    var post = await _repo.GetPost(postId, currentUserId);
                    if(post==null)
                    {
                        await _cacheService.RemoveFromOrderedHash(postsCacheKey, postId);
                    }
                    else
                    {
                        postDTOs.Add(post);
                        await _cacheService.CreateHashSetFrom(postId, post,delta);
                        await _cacheService.SetCacheValueAsync(likesCacheKey,post.Liked,delta);
                    }
                    
                }
            }
            return postDTOs;
        }
        var list = await _repo.GetPosts(userId, 0x7FFFFFFF, 0, currentUserId);
        foreach(var post in list)
        {
            string postId = post.PostId;
            string likesCacheKey = $"liked:{postId}+{currentUserId}";
            await _cacheService.CreateHashSetFrom(postId, post,delta);
            await _cacheService.SetCacheValueAsync(likesCacheKey,post.Liked,delta);
            await _cacheService.AddToOrderedHash(postsCacheKey, _ => post.Timestamp, post.PostId,delta);
        }
        return list;
    }


    public async Task DeletePost(string postId,string userId)
    {
        var postDto = await _repo.DeletePost(postId);
        await _cacheService.RemoveCacheValueAsync(postId);
        string postsCacheKey = $"{postDto.PostedById}:posts";
        await _cacheService.RemoveFromOrderedHash(postsCacheKey, postId);
    }

    public async Task<List<PostDTO>> GetFeed(string userId, int count)
    {
        var cacheKey = $"{userId}:feed";
        TimeSpan delta = TimeSpan.FromSeconds(120);
        if (_cacheService.KeyExists(cacheKey))
        {
            var postIds = await _cacheService.GetOrderedHash<string>(cacheKey);
            _cacheService.ExtendKey(cacheKey,delta);
            List<PostDTO> postDTOs = new(postIds.Count);
            foreach(string postId in postIds)
            {
                string likesCacheKey = $"liked:{postId}+{userId}";
                if(_cacheService.KeyExists(postId) && _cacheService.KeyExists(likesCacheKey))
                {
                    var post = await _cacheService.GetHashSet<PostDTO>(postId);
                    post.Liked = await _cacheService.GetCacheValueAsync<bool>(likesCacheKey);
                    postDTOs.Add(post);
                    _cacheService.ExtendKey(postId, delta);
                    _cacheService.ExtendKey(likesCacheKey, delta);
                }
                else
                {
                    var post = await _repo.GetPost(postId, userId);
                    if(post==null)
                    {
                        await _cacheService.RemoveFromOrderedHash(cacheKey, postId);
                    }
                    else
                    {
                        postDTOs.Add(post);
                        await _cacheService.CreateHashSetFrom(postId, post,delta);
                        await _cacheService.SetCacheValueAsync(likesCacheKey,post.Liked,delta);
                    }  
                }
            }

            
            return postDTOs;
        } 
        var list = await _repo.GetFeed(userId, count);
        foreach(var post in list)
        {
            string postId = post.PostId;
            string likesCacheKey = $"liked:{postId}+{userId}";
            await _cacheService.CreateHashSetFrom(postId, post,delta);
            await _cacheService.SetCacheValueAsync(likesCacheKey,post.Liked,delta);
            await _cacheService.AddToOrderedHash(cacheKey, _ => post.Timestamp, post.PostId,delta);
        }
        return list;
    }

    public async Task LikePost(string userId, string postId)
    {
        TimeSpan delta = TimeSpan.FromSeconds(120);
        var (state,user)= await _repo.LikePost(userId, postId);
        string postCacheKey = $"{postId}";
        string likeCacheKey = $"liked:{postId}+{userId}";
        _cacheService.IncrementHashField(postCacheKey,"Likes",state?1:-1);
        await _cacheService.SetCacheValueAsync(likeCacheKey, state, delta);
        string postLikesCacheKey = $"{postId}:likes";
        if (state && _cacheService.KeyExists(postLikesCacheKey))
            await _cacheService.AddToListHeadAsync(postLikesCacheKey, userId, delta);
        else if (!state && _cacheService.KeyExists(postLikesCacheKey))
            await _cacheService.RemoveFromList(postLikesCacheKey, userId);
    }

    public async Task LikeComment(string commentId,string userId,string postId)
    {
        TimeSpan delta = TimeSpan.FromSeconds(120);
        var (state,user) =await _repo.LikePost(userId, commentId);
        string commentCacheKey = $"{commentId}";
        string likeCacheKey = $"liked:{commentId}+{userId}";
        _cacheService.IncrementHashField(commentCacheKey, "Likes", state ? 1 : -1);
        await _cacheService.SetCacheValueAsync(likeCacheKey, state, delta);
        string commentLikesCacheKey = $"{commentId}:likes";
        if (state && _cacheService.KeyExists(commentLikesCacheKey))
            await _cacheService.AddToListHeadAsync(commentLikesCacheKey, userId, delta);
        else if (!state && _cacheService.KeyExists(commentLikesCacheKey))
            await _cacheService.RemoveFromList(commentLikesCacheKey, userId);
    }

    public async Task DeleteComment(string commentId,string userId,string postId)
    {
        await _repo.DeletePost(commentId);
    }
    
    public async Task<List<UserDTO>> GetLikes(string postId,string userId)
    {
        TimeSpan delta = TimeSpan.FromSeconds(120);
        var cacheKey = $"{postId}:likes";
        if(_cacheService.KeyExists(cacheKey))
        {
            var cachedLikes = await _cacheService.GetListAsync<string>(cacheKey);
            List<UserDTO> users = new(cachedLikes.Count());
            foreach(string likeUserId in cachedLikes)
            {
                string userCacheKey = $"profile:{likeUserId}";
                if(_cacheService.KeyExists(userCacheKey))
                {
                    var user = await _cacheService.GetHashSet<UserDTO>(cacheKey);
                    _cacheService.ExtendKey(userCacheKey, delta);
                    users.Add(user);
                }
                else
                {
                    var user = await _userService.GetUserDTO(likeUserId,userId);
                    if(user==null)
                    {
                        await _cacheService.RemoveFromList(cacheKey, likeUserId);
                    }
                    await _cacheService.CreateHashSetFrom(cacheKey, user, delta);
                    users.Add(user!);
                }
            }
            return users;
        } 
        var likes = await _repo.GetLikes(postId,userId);
        await _cacheService.AddToListFrom(cacheKey, likes.Select(u => u.UserId).ToList(), TimeSpan.FromMinutes(2));
        return likes;
    }
    public async Task AddComment(Post comment, string userId, string postId)
    {
        var commentDTO=await _repo.AddComment(comment, userId, postId);
        TimeSpan delta = TimeSpan.FromSeconds(120);
        string commentsCacheKey = $"{postId}:comments";
        if(_cacheService.KeyExists(commentsCacheKey))
        {
            await _cacheService.AddToOrderedHash(commentsCacheKey, c => commentDTO.Likes, commentDTO.PostId,
            delta);
            string commentCacheKey = $"{commentDTO.PostId}";
            await _cacheService.CreateHashSetFrom(commentCacheKey, commentDTO, delta);
            await _cacheService.SetCacheValueAsync($"liked:{commentCacheKey}+{userId}", "false", delta);
        }
    }

    public async Task<List<PostDTO>> GetComments(string postId,string userId)
    {
        var cacheKey = $"{postId}:comments";
        TimeSpan delta = TimeSpan.FromSeconds(120);
        List<PostDTO> comments;
        if(_cacheService.KeyExists(cacheKey))
        {
            var list = await _cacheService.GetOrderedHash<string>(cacheKey);
            comments= new(list.Count);
            foreach(string commentId in list)
            {
                string likedCacheKey = $"liked:{commentId}+{userId}";
                if(_cacheService.KeyExists(commentId)&&_cacheService.KeyExists(likedCacheKey))
                {
                    var comment = await _cacheService.GetHashSet<PostDTO>(commentId);
                    comment.Liked=await _cacheService.GetCacheValueAsync<bool>(likedCacheKey);
                    comments.Add(comment);
                }
                else
                {
                    var comment =await _repo.GetPost(commentId, userId);
                    if(comment==null)
                    {
                        await _cacheService.RemoveFromOrderedHash(cacheKey, commentId);
                    }
                    else
                    {
                        await _cacheService.CreateHashSetFrom(commentId,comment,delta);
                        await _cacheService.SetCacheValueAsync(likedCacheKey, comment.Liked);
                        comments.Add(comment);
                    }
                }
            }
            return comments;
        }
        comments = await _repo.GetComments(postId,userId);
        foreach(var comment in comments)
        {
            string commentId = comment.PostId;
            string likesCacheKey = $"liked:{commentId}+{userId}";
            await _cacheService.CreateHashSetFrom(commentId, comment,delta);
            await _cacheService.SetCacheValueAsync(likesCacheKey,comment.Liked,delta);
            await _cacheService.AddToOrderedHash(cacheKey, _ => comment.Timestamp, comment.PostId,delta);
        }
        return comments;
    }

}