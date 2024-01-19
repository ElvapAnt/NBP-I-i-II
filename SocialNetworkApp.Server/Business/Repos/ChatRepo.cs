using Microsoft.OpenApi.Models;
using Neo4j.Driver;
using SocialNetworkApp.Server.Data;
using SocialNetworkApp.Server.Data.Entities;
using SocialNetworkApp.Server.Error;

namespace SocialNetworkApp.Server.Business.Repos;

public class ChatRepo(IDriver driver)
{
    private IDriver _driver = driver;

    public async Task<Chat> CreateChat(string name,bool isGroup = false,params string[] memberIds)
    {
        using var session = _driver.AsyncSession();
        var chat = new
        {
            ChatId = "chat:" + Guid.NewGuid().ToString(),
            CreationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Name = name,
            LatestTimestamp=0
        };
        //(u.UserId + ' {\"Username: '+u.Username+', Thumbnail: '+u.Thumbnail+'}') 
        string query = "CREATE (chat:Chat $chat) WITH chat " +
        "UNWIND $memberIds as memberId MATCH (u:User{UserId:memberId}) CREATE (u)-[:MEMBER_OF]->(chat) WITH u,chat " +
        "SET chat.Members = COALESCE(chat.Members, []) + (u.UserId + ' '+u.Username+' '+u.Thumbnail) "+
        "WITH chat RETURN chat";
        var parameters = new { chat, memberIds };
        var result = await session.RunAsync(query, parameters);
        bool success=await result.FetchAsync();
        if (!success) throw new CustomException("Error fetching chat.");
        var newChat = RecordMapper.ToChat(result.Current, "chat");
        return newChat;
    }

    public async Task<Chat?> FindChat(string memberId1,string memberId2)
    {
        using var session = _driver.AsyncSession();
        var chat = new
        {
            ChatId = "chat:" + Guid.NewGuid().ToString(),
            CreationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Name = "",
            LatestTimestamp=0
        };
        string query = "MATCH (c:Chat) " +
        "WHERE (:User{UserId:$userId1})-[:MEMBER_OF]->(c) " +
        "AND (:User{UserId:$userId2})-[:MEMBER_OF]->(c) " +
        "AND NOT c: GroupChat "+
        "RETURN c";
        var parameters = new { userId1 = memberId1, userId2 = memberId2};
        var result = await session.RunAsync(query, parameters);
        var valid = await result.FetchAsync();
        if (!valid)
            return null;
        var record = result.Current;
        return RecordMapper.ToChat(record, "c");

    }

    public async Task AddMesage(Message message,string chatId)
    {
        using var session = _driver.AsyncSession();
        message.MessageId = "message:" + Guid.NewGuid().ToString();
        message.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string query = "CREATE (msg:Message $message) WITH msg MATCH (chat:Chat{ChatId:$chatId}) " +
        "CREATE (chat)-[:HAS_MSG]->(msg) SET chat.LatestTimestamp = $timestamp";
        var parameters = new { message, chatId, timestamp = message.Timestamp };
        await session.RunAsync(query, parameters);
    }

    public async Task<List<Chat>> GetInbox(string userId)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (user:User{UserId:$userId})-[:MEMBER_OF]->(chat:Chat) WITH chat"
        + " ORDER BY chat.LatestTimestamp DESC, chat.ChatId DESC return chat";
        var parameters = new { userId };
        var result = await session.RunAsync(query, parameters);
        var list = await result.ToListAsync();
        return RecordMapper.ToChatList(list, "chat");
    }

    public async Task<List<Message>> GetMessages(string chatId)
    {
        using var session = _driver.AsyncSession();
        string query = "MATCH (chat:Chat{ChatId:$chatId})-[h:HAS_MSG]->(msg:Message) WITH msg "+
        "ORDER BY msg.Timestamp DESC, msg.MessageId DESC SET msg.Read=TRUE RETURN msg";
        var parameters = new { chatId };
        var result = await session.RunAsync(query, parameters);
        var list = await result.ToListAsync();
        return RecordMapper.ToMessageList(list, "msg");
    }

    public async Task DeleteMessage(string messageId)
    {
        using var session = _driver.AsyncSession();

        string query = "MATCH (message:Message{MessageId:$messageId}) DETACH DELETE message";
        var parameters = new { messageId };
        await session.RunAsync(query, parameters);
    }

    public async Task EditMessage(string messageId,string newContent)
    {
        using var session = _driver.AsyncSession();

        string query = "MATCH (message:Message{MessageId:$messageId}) SET message.Edited=true SET message.Content = $newContent";
        var parameters = new {messageId,newContent};
        await session.RunAsync(query, parameters);
    }
}
