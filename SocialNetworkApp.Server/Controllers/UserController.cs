using Microsoft.AspNetCore.Mvc;
using SocialNetworkApp.Server.Data.Entities;
using SocialNetworkApp.Server.Business.Services;

namespace SocialNetworkApp.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(UserService service) : ControllerBase
{
    private readonly UserService _service = service;

    [HttpGet("LogIn/{username}/{password}")]
    public async Task<IActionResult> LogIn([FromRoute]string username,[FromRoute]string password)
    {
        var res = await _service.LogIn(username, password);
        return res != null ? Ok(res) : BadRequest("Error logging in");
    }
    [HttpPost("AddUser")]
    public async Task<IActionResult> AddUser([FromBody] User user)
    {
        await _service.AddUser(user);

        return Ok(user.UserId);
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

    [HttpPut("UpdateUsernameAndPassword/{userId}/{newUsername}")]
    public async Task<IActionResult> UpdateUsername([FromRoute]string userId,[FromRoute]string newUsername)
    {
        await _service.UpdateUsername(userId, newUsername);
        return Ok("User updated.");
    }

    [HttpPut("UpdateProfilePicture/{userId}/{profilePic}")]
    public async Task<IActionResult> UpdateProfilePic([FromRoute]string userId,[FromRoute]string profilePic)
    {
        await _service.UpdateThumbnail(userId, profilePic);
        return Ok("User updated");
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

    [HttpGet("SearchForUsers/{usernamePattern}")]
    public async Task<IActionResult> SearchForUsers([FromRoute] string usernamePattern)
    {
        return Ok(await _service.SearchForUsers(usernamePattern));
    }

    
}