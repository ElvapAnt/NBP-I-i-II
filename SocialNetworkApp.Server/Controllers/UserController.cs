using Microsoft.AspNetCore.Mvc;
using SocialNetworkApp.Server.Entities;
using SocialNetworkApp.Services;

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
}