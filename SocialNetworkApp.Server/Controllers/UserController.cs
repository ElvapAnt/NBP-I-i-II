using Microsoft.AspNetCore.Mvc;
using SocialNetworkApp.Server.Data.Entities;
using SocialNetworkApp.Business.Services;

namespace SocialNetworkApp.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(UserService service) : ControllerBase
{
    private readonly UserService _service = service;
    [HttpPost("AddUser")]
    public async Task<IActionResult> AddUser([FromBody] User user)
    {
        await _service.AddUser(user);

        return Ok("User added.");
    }

     [HttpPost("GetUser/{userId}")]
    public async Task<IActionResult> GetUser([FromRoute] string userId)
    {
        var user = await _service.GetUser(userId);
        return Ok(user);
    }

    [HttpPost("GetUserByUsername/{username}")]
    public async Task<IActionResult> GetUserByUsername([FromRoute] string username)
    {
        var user = await _service.GetUserByUsername(username);
        return Ok(user);
    }

    [HttpDelete("DeleteUser/{userId}")]
    public async Task<IActionResult> DeleteUser([FromRoute]string userId)
    {
        await _service.DeleteUser(userId);
        return Ok($"User : {userId} has been deleted.");
    }

    [HttpPut("UpdateUser")]
    public async Task<IActionResult> UpdateUser([FromBody]User user)
    {
        await _service.UpdateUser(user);
        return Ok("User updated.");
    }

    [HttpGet("GetFriends/{userId}")]
    public async Task<IActionResult> GetFriends([FromRoute]string userId,[FromQuery]int count=0x7FFFFFFF,[FromQuery]int skip=0)
    {
        return Ok(await _service.GetFriends(userId, count, skip));
    }

    [HttpGet("GetRecommendedFriends/{userId}")]
    public async Task<IActionResult> GetRecommendedFriends([FromRoute]string userId,[FromQuery]int count=0x7FFFFFFF,[FromQuery]int skip=0)
    {
        return Ok(await _service.GetRecommendedFriends(userId, count, skip));
    }

    
}