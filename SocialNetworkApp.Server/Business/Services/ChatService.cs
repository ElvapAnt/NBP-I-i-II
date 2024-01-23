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

    public async Task<Tuple<Chat,bool>> FindOrCreateChat(string recipientId,string senderId)
    {
        var chat = await _repo.FindChat(recipientId, senderId);
        if (chat==null)
        {
            chat = await _repo.CreateChat("",false,recipientId,senderId);

            string inboxCacheKey = $"{senderId}:inbox";
            await _cacheService.AddToListHeadAsync(inboxCacheKey,chat,TimeSpan.FromMinutes(2));

            inboxCacheKey = $"{recipientId}:inbox";
            await _cacheService.AddToListHeadAsync(inboxCacheKey, chat,TimeSpan.FromMinutes(2));
            return Tuple.Create(chat, true);
        }
        return Tuple.Create(chat, false);
    }
    public async Task<Chat?> SendMesage(Message message,string recipientId)
    {
        Chat? chat = null;
        if(recipientId.StartsWith("user:"))
        {
            
            var (chat2,created) = await FindOrCreateChat(recipientId, message.SenderId);
            chat = chat2!;
            recipientId = chat.ChatId;
        }

        await _repo.AddMesage(message, recipientId);

        var serializedMessage = JsonConvert.SerializeObject(message);
        await _cacheService.PublishAsync($"{recipientId}", serializedMessage);
        var cacheKey = $"{recipientId}:messages";
        await _cacheService.AddToListHeadAsync(cacheKey, message,TimeSpan.FromMinutes(2));
        //mozda da procesira notifikaciju za poruku
        await _cacheService.EnqueueMessageAsync("messageQueue", serializedMessage);

        return chat;
    }

    public async Task<List<Chat>> GetInbox(string userId)
    {
        var cacheKey = $"{userId}:inbox";
        var cachedInbox = await _cacheService.GetListAsync<Chat>(cacheKey);
        
        if(cachedInbox!=null && cachedInbox.Any())
            return cachedInbox.ToList();
        
        var inbox  = await _repo.GetInbox(userId);
        await _cacheService.AddToListFrom(cacheKey, inbox, TimeSpan.FromMinutes(5));
        return inbox;
    }

    public async Task<List<Message>> GetMessages(string chatId)
    {
        var cacheKey = $"{chatId}:messages";
        var cachedMessages = await _cacheService.GetListAsync<Message>(cacheKey);
        
        if(cachedMessages!=null && cachedMessages.Any())
            return cachedMessages.ToList();

        var messages = await _repo.GetMessages(chatId);
        await _cacheService.AddToListFrom(cacheKey, messages, TimeSpan.FromMinutes(2));
        return messages;
    }

    public async Task DeleteMessage(string messageId)
    {

        string chatId = await _repo.DeleteMessage(messageId);
        
        var cacheKey = $"{chatId}:messages";
        await _cacheService.RemoveCacheValueAsync(cacheKey);

      /*   var message = new { MessageId = messageId, Action = "delete" };
        var serializedMessage = JsonConvert.SerializeObject(message);
        await _cacheService.PublishAsync($"chat:{chatId}", serializedMessage); */
    }



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