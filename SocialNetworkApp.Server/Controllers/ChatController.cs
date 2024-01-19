using Microsoft.AspNetCore.Mvc;
using SocialNetworkApp.Server.Business.Services;
using SocialNetworkApp.Server.Data.Entities;

namespace SocialNetworkApp.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatController(ChatService service):ControllerBase
{
    private ChatService _service = service;
    public class ChatDTO
    {
        public string Name { get; set; } = "";
        public string[] MemberIds { get; set; } = [];

        public void Deconstruct(out string name, out string[] memberIds)
        {
            name = Name;
            memberIds = MemberIds;
        }
    }
    [HttpPost]
    [Route("CreateChat")]
    public async Task<IActionResult> CreateChat([FromBody]ChatDTO dto)
    {
        var (name, memberIds) = dto;
        var chat =await _service.CreateChat(name, memberIds);
        return Ok(chat.ChatId);
    }

    [HttpPost]
    [Route("SendMessage/{recipientId}")]
    public async Task<IActionResult> SendMessage([FromBody]Message message,string recipientId)
    {
        Chat? chat = await _service.SendMesage(message, recipientId);
        return Ok(chat);
    }

    [HttpGet]
    [Route("GetInbox/{userId}")]
    public async Task<IActionResult> GetInbox([FromRoute]string userId)
    {
        var list =await _service.GetInbox(userId);
        return Ok(list);
    }

    [HttpGet]
    [Route("GetMessages/{chatId}")]
    public async Task<IActionResult> GetMessages([FromRoute]string chatId)
    {
        var list = await _service.GetMessages(chatId);
        return Ok(list);
    }

    [HttpDelete]
    [Route("DeleteMessage/{messageId}")]
    public async Task<IActionResult> DeleteMessage([FromRoute]string messageId)
    {
        await _service.DeleteMessage(messageId);
        return Ok("Message deleted");
    }

    [HttpPut]
    [Route("EditMessage/{messageId}/{newContent}")]
    public async Task<IActionResult> EditMessage([FromRoute]string messageId,[FromRoute]string newContent)
    {
        await _service.EditMessage(messageId, newContent);
        return Ok("Message edited.");
    }
    
}