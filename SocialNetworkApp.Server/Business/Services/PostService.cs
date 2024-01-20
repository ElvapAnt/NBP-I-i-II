using SocialNetworkApp.Server.Business.Repos;
using SocialNetworkApp.Server.Data.Entities;

namespace SocialNetworkApp.Server.Business.Services;

public class PostService(PostRepo repo)
{
    private readonly PostRepo _repo = repo;
    
    public async Task AddPost(Post post,string userId)
    {
        await _repo.AddPost(post,userId);
    }

    public async Task<List<PostDTO>> GetPosts(string userId,int count,int skip,string currentUserId)
    {
        return await _repo.GetPosts(userId,count,skip,currentUserId);
    }

    public async Task DeletePost(string postId)
    {
        await _repo.DeletePost(postId);
    }

    public async Task<List<PostDTO>> GetFeed(string userId,int count)
    {
        return await _repo.GetFeed(userId, count);
    }

    public async Task LikePost(string userId,string postId)
    {
        await _repo.LikePost(userId, postId);
    }

    public async Task<List<UserDTO>> GetLikes(string postId,string userId)
    {
        return await _repo.GetLikes(postId,userId);
    }
    //COMMENT
     public async Task AddComment(Post comment, string userId,string postId)
    {
        await _repo.AddComment(comment, userId, postId);
    }

    public async Task<List<PostDTO>> GetComments(string postId,string userId)
    {
        return await _repo.GetComments(postId,userId);
    }

}