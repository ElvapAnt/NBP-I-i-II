
using Microsoft.AspNetCore.Mvc;
using SocialNetworkApp.Server.Business.Services;
using SocialNetworkApp.Server.Data.Entities;

namespace SocialNetworkApp.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class NotificationController(NotificationService service):ControllerBase
{
    private readonly NotificationService _service = service;

    [HttpPost]
    [Route("AddRequest/{fromId}/{toId}")]
    public async Task<IActionResult> AddRequest([FromBody]Notification notification,[FromRoute]string fromId,[FromRoute]string toId)
    {
        await _service.AddRequest(notification, fromId, toId);
        return Ok("Request added");
    }

    [HttpGet]
    [Route("GetReceivedRequests/{userId}")]
    public async Task<IActionResult> GetReceivedRequests([FromRoute]string userId,[FromQuery]int count=0x7FFFFFFF,int skip=0)
    {
        return Ok(await _service.GetReceivedRequests(userId,count,skip));
    }

    [HttpDelete]
    [Route("DeleteRequest/{requestId}/{userId}")]
    public async Task<IActionResult> DeleteRequest([FromRoute]string requestId,[FromRoute]string userId)
    {
        await _service.DeleteRequest(requestId,userId);
        return Ok("Request deleted.");
    }
}