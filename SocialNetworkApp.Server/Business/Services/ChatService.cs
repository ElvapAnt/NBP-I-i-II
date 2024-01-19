using SocialNetworkApp.Server.Business.Repos;
using SocialNetworkApp.Server.Data.Entities;

namespace SocialNetworkApp.Server.Business.Services;

public class ChatService(ChatRepo repo)
{
    private readonly ChatRepo _repo = repo;

    public async Task<Chat> CreateChat(string name,params string[] memberIds)
    {
        return await _repo.CreateChat(name, false,memberIds);
    }

    private async Task<Chat> FindOrCreateChat(string recipientId,string senderId)
    {
        return await _repo.FindChat(recipientId, senderId)??await _repo.CreateChat("",false,recipientId,senderId);
    }
    public async Task<Chat?> SendMesage(Message message,string recipientId)
    {
        Chat? chat = null;
        if(recipientId.StartsWith("user:"))
        {
            chat = await FindOrCreateChat(recipientId, message.SenderId);
            recipientId = chat.ChatId;
        }
        await _repo.AddMesage(message, recipientId);
        return chat;
    }

    
    public async Task<List<Chat>> GetInbox(string userId)
    {
        return await _repo.GetInbox(userId);
    }

    public async Task<List<Message>> GetMessages(string chatId)
    {
        return await _repo.GetMessages(chatId);
    }

    public async Task DeleteMessage(string messageId)
    {
        await _repo.DeleteMessage(messageId);
    }

    public async Task EditMessage(string messageId,string newContent)
    {
        await _repo.EditMessage(messageId, newContent);
        
    }
}