using Microsoft.AspNetCore.Mvc;
using SocialNetworkApp.Server.Data.Entities;
using SocialNetworkApp.Server.Business.Services;
using SocialNetworkApp.Server.Business.Services.Redis;

namespace SocialNetworkApp.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(UserService service, ICacheService cacheService) : ControllerBase
{
    private readonly UserService _service = service;
    private readonly ICacheService _cacheService = cacheService;

    [HttpGet("LogIn/{username}/{password}")]
    public async Task<IActionResult> LogIn([FromRoute]string username,[FromRoute]string password)
    {
        var res = await _service.LogIn(username, password);
        if (res!=null)
        {
            var sessionToken = $"{res.UserId}:{res.Password}";
            if (string.IsNullOrEmpty(sessionToken) || await _cacheService.GetCacheValueAsync<User>(sessionToken) == null)
            {
                await _cacheService.SetCacheValueAsync(sessionToken, res, TimeSpan.FromMinutes(15));
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("SessionToken", sessionToken, cookieOptions);

            res.Password = "";
            return Ok(res);
        }
        else
        {
            return BadRequest("Error logging in");
        }
    }
    [HttpPost("AddUser")]
    public async Task<IActionResult> AddUser([FromBody] User user)
    {
        await _service.AddUser(user);

        return Ok(user.UserId);
    }

    [HttpGet("GetUser/{userId}")]
    public async Task<IActionResult> GetUser([FromRoute] string userId)
    {
        var sessionToken = Request.Cookies["SessionToken"];

        if (string.IsNullOrEmpty(sessionToken) || await _cacheService.GetCacheValueAsync<User>(sessionToken) == null)
        {
            return Unauthorized("No active session found. Please log in.");
        }

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

    [HttpPut("UpdateUsername/{userId}/{newUsername}")]
    public async Task<IActionResult> UpdateUsername([FromRoute]string userId,[FromRoute]string newUsername)
    {
        await _service.UpdateUsername(userId, newUsername);
        return Ok("User updated.");
    }

    [HttpPut("UpdateProfilePicture/{userId}")]
    public async Task<IActionResult> UpdateProfilePic([FromRoute]string userId,[FromBody]string profilePic)
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
    public async Task<IActionResult> SearchForUsers([FromRoute] string usernamePattern,string userId)
    {
        return Ok(await _service.SearchForUsers(usernamePattern,userId));
    }

    [HttpPut("AddFriend/{userId1}/{userId2}")]
    public async Task<IActionResult> AddFriend([FromRoute]string userId1,[FromRoute]string userId2)
    {
        await _service.AddFriend(userId1, userId2);
        return Ok("Friend added.");
    }

    
}