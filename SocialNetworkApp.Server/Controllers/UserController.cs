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
            var sessionToken = "token:" + Guid.NewGuid().ToString();
            if (await _cacheService.GetCacheValueAsync<string>(sessionToken) == null)
            {
                await _cacheService.SetCacheValueAsync<string>(sessionToken, res.UserId, TimeSpan.FromHours(1));
            }

/*             var cookieOptions = new CookieOptions
            {
                HttpOnly = false,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Secure=false,

            };
            Response.Cookies.Append("SessionToken", sessionToken, cookieOptions);
*/
            res.Password = ""; 
            return Ok(Tuple.Create<User, string>(res, sessionToken));
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

    [HttpGet("GetUser/{userId}/{sessionToken}")]
    public async Task<IActionResult> GetUser([FromRoute] string userId,[FromRoute]string sessionToken)
    {
        //var sessionToken = Request.Cookies["SessionToken"];
        string? userId2 = await _cacheService.GetCacheValueAsync<string>(sessionToken);

        if (userId2==null)
        {
            return Unauthorized("No active session found. Please log in.");
        }

        var user = await _service.GetUserDTO(userId,userId2);
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

    [HttpGet("SearchForUsers/{usernamePattern}/{userId}")]
    public async Task<IActionResult> SearchForUsers([FromRoute] string usernamePattern,[FromRoute]string userId)
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