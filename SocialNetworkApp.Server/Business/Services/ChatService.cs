using Newtonsoft.Json;
using SocialNetworkApp.Server.Business.Repos;
using SocialNetworkApp.Server.Business.Services.Redis;
using SocialNetworkApp.Server.Data.Entities;
using System.Runtime.InteropServices;

namespace SocialNetworkApp.Server.Business.Services;

public class ChatService(ChatRepo repo, ICacheService cacheService)
{
    private readonly ChatRepo _repo = repo;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<Chat> CreateChat(string name, params string[] memberIds)
    {
        return await _repo.CreateChat(name, false, memberIds);
    }

    private async Task<Chat> FindOrCreateChat(string recipientId,string senderId)
    {
        return await _repo.FindChat(recipientId, senderId) ??
            await _repo.CreateChat("",false,recipientId,senderId);
    }
    public async Task<Chat?> SendMesage(Message message,string recipientId)
    {
        Chat? chat = null;
        if(recipientId.StartsWith("user:"))
        {
            chat = await FindOrCreateChat(recipientId, message.SenderId);
            recipientId = chat.ChatId;
        }

        var cacheKey = $"{recipientId}:messages";
        await _cacheService.RemoveCacheValueAsync(cacheKey);
       
        await _repo.AddMesage(message, recipientId);

        var serializedMessage = JsonConvert.SerializeObject(message);
        await _cacheService.PublishAsync($"{recipientId}", serializedMessage);

        //mozda da procesira notifikaciju za poruku
        await _cacheService.EnqueueMessageAsync("messageQueue", serializedMessage);

        return chat;
    }

    public async Task<List<Chat>> GetInbox(string userId)
    {
        var cacheKey = $"{userId}:inbox";
        var cachedInbox = await _cacheService.GetCacheValueAsync<List<Chat>>(cacheKey);
        
        if(cachedInbox!=null)
            return cachedInbox;
        
        var inbox  = await _repo.GetInbox(userId);
        await _cacheService.SetCacheValueAsync(cacheKey, inbox, TimeSpan.FromMinutes(30));
        return inbox;
    }

    public async Task<List<Message>> GetMessages(string chatId)
    {
        var cacheKey = $"{chatId}:messages";
        var cachedMessages = await _cacheService.GetCacheValueAsync<List<Message>>(cacheKey);
        
        if(cachedMessages!=null)
            return cachedMessages;

        var messages = await _repo.GetMessages(chatId);
        await _cacheService.SetCacheValueAsync(cacheKey, messages, TimeSpan.FromMinutes(30));
        return messages;
    }

    //za brisanje i editovanje bi trebalo da se updateuje kes 
    //ili da se prosledi i chatID cisto da bi mogao kes da se invalidira ili edituje
    public async Task DeleteMessage(string messageId)
    {

        await _repo.DeleteMessage(messageId);
        
       /* var cacheKey = $"chat:{chatId}:messages";
        await _cacheService.RemoveCacheValueAsync(cacheKey);

        var message = new { MessageId = messageId, Action = "delete" };
        var serializedMessage = JsonConvert.SerializeObject(message);
        await _cacheService.PublishAsync($"chat:{chatId}", serializedMessage);*/
    }

    // FYI za editovanje : 
    // If you're using a front-end framework like React,
    // this would typically trigger a re-render of the component displaying the message,
    // showing the updated content.

    public async Task EditMessage(string messageId,string newContent)
    {
        await _repo.EditMessage(messageId, newContent);
        
       /* var message = new Message
        {
            MessageId = messageId,
            Content = newContent,
            Edited = true
        };
        var serializedMessage = JsonConvert.SerializeObject(message);
        await _cacheService.PublishAsync($"chat:{chatId}", serializedMessage);*/

       /* var cacheKey = $"chat:{chatId}:messages";
        await _cacheService.RemoveCacheValueAsync(cacheKey);

        var message = new { MessageId = messageId, Action = "edit", NewContent = newContent };
        var serializedMessage = JsonConvert.SerializeObject(message);
        await _cacheService.PublishAsync($"chat:{chatId}", serializedMessage);*/
    }
}