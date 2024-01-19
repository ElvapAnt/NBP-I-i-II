using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Neo4j.Driver;
using Neo4jClient;
using Newtonsoft.Json;
using NRedisStack.Graph;
using SocialNetworkApp.Server.Data.Entities;

namespace SocialNetworkApp.Data;

public static class RecordMapper
{

    public static User ToUser(IRecord record,string nodeKey="user")
    {
        INode node = record[nodeKey].As<INode>();
        return new User
        {
            UserId = node["UserId"].As<string>(),
            Username = node["Username"].As<string>(),
            Name = node["Name"].As<string>(),
            Bio = node["Bio"].As<string>(),
            Email = node["Email"].As<string>(),
            Password=node["Password"].As<string>(),
            Thumbnail=node["Thumbnail"].As<string>()
        };
    }

    public static List<User> ToUserList(List<IRecord> records,string nodeKey)
    {
        return records.Select(record=>ToUser(record,nodeKey)
        ).ToList();
    }

    public static Post ToPost(IRecord record,string nodeKey="post")
    {
        INode node = record[nodeKey].As<INode>();
        return new Post
        {
            PostId = node["PostId"].As<string>(),
            PostedBy = node["PostedBy"].As<string>(),
            Timestamp = node["Timestamp"].As<long>(),
            Likes = node["Likes"].As<int>(),
            Content = node["Content"].As<string>(),
            MediaURL=node["MediaURL"].As<string>()
        };
    }

    public static List<Post> ToPostList(List<IRecord> records,string nodeKey)
    {
        return records.Select(record => ToPost(record, nodeKey)).ToList();
    }

    public static Chat ToChat(IRecord record,string nodeKey)
    {
        var node = record[nodeKey].As<INode>();
        return new Chat
        {
            ChatId = node["ChatId"].As<string>(),
            Name = node["Name"].As<string>(),
            CreationTimestamp = node["CreationTimestamp"].As<long>(),
            LatestTimestamp = node["LatestTimestamp"].As<long>(),
            Members = node["Members"].As<List<string>>().ToDictionary(str =>
            {
                int index = str.IndexOf(' ');
                string userId = str[..index];
                return userId;
            }, str =>
            {
                string[] chatUserParams = str.Split(' ');
                ChatUser chatUser = new ChatUser
                {
                    Username=chatUserParams[1],
                    Thumbnail=chatUserParams[2]
                };
                return chatUser;
            })
                    };
    }
    public static List<Chat> ToChatList(List<IRecord> records, string nodeKey)
    {
        return records.Select(record => ToChat(record, nodeKey)).ToList();
    }

    public static Message ToMessage(IRecord record, string nodeKey)
    {
        var node = record[nodeKey].As<INode>();
        return new Message
        {
            MessageId=node["MessageId"].As<string>(),
            Timestamp=node["Timestamp"].As<long>(),
            SenderId=node["SenderId"].As<string>(),
            Content=node["Content"].As<string>(),
            Read=node["Read"].As<bool>(),
            Edited=node["Edited"].As<bool>()
        };
    }

    public static List<Message> ToMessageList(List<IRecord> records, string nodeKey)
    {
        return records.Select(record => ToMessage(record, nodeKey)).ToList();
    }

    public static Notification ToNotification(IRecord record,string nodeKey)
    {
        var node = record[nodeKey].As<INode>();
        return new Notification
        {
            NotificationId=node["NotificationId"].As<string>(),
            Timestamp=node["Timestamp"].As<long>(),
            Viewed=node["Viewed"].As<bool>(),
            From=node["From"].As<string>(),
            URL=node["URL"].As<string>(),
            Content =node["Content"].As<string>()
        };
    }

    public static List<Notification> ToNotificationList(List<IRecord> records,string nodeKey)
    {
        return records.Select(record => ToNotification(record, nodeKey)).ToList();
    }
}