using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkApp.Server.Business.Services;
using SocialNetworkApp.Server.Data.Entities;

namespace SocialNetworkApp.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class PostController(PostService service):ControllerBase
{
    private readonly PostService _service = service;

    [HttpGet("GetPosts/{userId}/{currentId}")]
    public async Task<IActionResult> GetPosts([FromRoute]string userId,[FromRoute]string currentId,[FromQuery]int count=0x7FFFFFFF,[FromQuery]int skip=0)
    {
        List<PostDTO> posts =await _service.GetPosts(userId,count,skip,currentId);
        return Ok(posts);
    }

    [HttpPost("AddPost/{userId}")]
    public async Task<IActionResult> AddPost([FromBody] Post post,[FromRoute]string userId)
    {
        await _service.AddPost(post, userId);
        return Ok("Posted succesfully.");
    }

    [HttpDelete("DeletePost/{postId}/{userId}")]
    public async Task<IActionResult> DeletePost([FromRoute]string postId,[FromRoute]string userId)
    {
        await _service.DeletePost(postId,userId);
        return Ok("Post deleted.");
    }

    [HttpPut("LikePost/{userId}/{postId}")]
    public async Task<IActionResult> LikePost([FromRoute]string userId,[FromRoute]string postId)
    {
        await _service.LikePost(userId, postId);
        return Ok("Like/Unlike submitted.");
    }

    [HttpPut("LikeComment/{userId}/{postId}/{commentId}")]
    public async Task<IActionResult> LikeComment(string userId,string postId,string commentId)
    {
        await _service.LikeComment(commentId, userId, postId);
        return Ok("Like/Unlike submitted");
    }

    [HttpGet("GetFeed/{userId}")]
    public async Task<IActionResult> GetFeed([FromRoute] string userId, [FromQuery] int count = 20)
    {
        List<PostDTO> list = await _service.GetFeed(userId,count);
        return Ok(list);
    }

    [HttpGet("GetLikes/{postId}/{userId}")]
    public async Task<IActionResult> GetLikes([FromRoute]string postId,[FromRoute]string userId)
    {
        List<UserDTO> likedBy = await _service.GetLikes(postId,userId);
        return Ok(likedBy);
    }

    [HttpPost("AddComment/{userId}/{postId}")]
    public async Task<IActionResult> AddComment([FromBody] Post post,[FromRoute]string userId,[FromRoute]string postId)
    {
        await _service.AddComment(post, userId,postId);
        return Ok("Posted succesfully.");
    }
    [HttpGet("GetComments/{postId}/{userId}")]
    public async Task<IActionResult> GetComments([FromRoute]string postId,[FromRoute]string userId)
    {
        return Ok(await _service.GetComments(postId,userId));
    }
}