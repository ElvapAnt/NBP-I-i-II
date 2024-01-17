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

    [HttpGet("GetPosts/{userId}")]
    public async Task<IActionResult> GetPosts([FromRoute]string userId,[FromQuery]int count=0x7FFFFFFF,[FromQuery]int skip=0)
    {
        List<Post> posts =await _service.GetPosts(userId,count,skip);

        return Ok(posts);
    }

    [HttpPost("AddPost/{userId}")]
    public async Task<IActionResult> AddPost([FromBody] Post post,[FromRoute]string userId)
    {
        await _service.AddPost(post, userId);
        return Ok("Posted succesfully.");
    }

    [HttpDelete("DeletePost/{postId}")]
    public async Task<IActionResult> DeletePost([FromRoute]string postId)
    {
        await _service.DeletePost(postId);
        return Ok("Post deleted.");
    }

    [HttpPut("LikePost/{userId}/{postId}")]
    public async Task<IActionResult> LikePost([FromRoute]string userId,[FromRoute]string postId)
    {
        await _service.LikePost(userId, postId);
        return Ok("Like/Unlike submitted.");
    }

    [HttpGet("GetFeed/{userId}")]
    public async Task<IActionResult> GetFeed([FromRoute] string userId, [FromQuery] int count = 20)
    {
        List<Post> list = await _service.GetFeed(userId,count);
        return Ok(list);
    }

    [HttpGet("GetLikes/{postId}")]
    public async Task<IActionResult> GetLikes([FromRoute]string postId)
    {
        List<User> likedBy = await _service.GetLikes(postId);
        return Ok(likedBy);
    }

    [HttpPost("AddComment/{userId}/{postId}")]
    public async Task<IActionResult> AddComment([FromBody] Post post,[FromRoute]string userId,[FromRoute]string postId)
    {
        await _service.AddComment(post, userId,postId);
        return Ok("Posted succesfully.");
    }
    [HttpGet("GetComments/{postId}")]
    public async Task<IActionResult> GetComments([FromRoute]string postId)
    {
        return Ok(await _service.GetComments(postId));
    }
}