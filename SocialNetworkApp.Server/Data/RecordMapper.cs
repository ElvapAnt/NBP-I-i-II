using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Neo4j.Driver;
using Neo4jClient;
using Newtonsoft.Json;
using NRedisStack.Graph;
using SocialNetworkApp.Server.Data.Entities;

namespace SocialNetworkApp.Server.Data;

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

    
    public static UserDTO ToUserDTO(IRecord record,string nodeKey="user")
    {
        var node = record[nodeKey].As<IDictionary<string,object>>();
        return new UserDTO()
        {
            UserId = node["UserId"].As<string>(),
            Username = node["Username"].As<string>(),
            Name = node["Name"].As<string>(),
            Bio = node["Bio"].As<string>(),
            Email = node["Email"].As<string>(),
            Thumbnail=node["Thumbnail"].As<string>(),
            IsFriend=node["IsFriend"].As<bool>(),
            SentRequest=node["SentRequest"].As<bool>(),
            RecievedRequest = node["RecievedRequest"].As<bool>()
        };
    }

    public static List<UserDTO> ToUserList(List<IRecord> records,string nodeKey)
    {
        return records.Select(record=>ToUserDTO(record,nodeKey)
        ).ToList();
    }

    public static PostDTO ToPost(IRecord record,string nodeKey="post")
    {
        var node = record[nodeKey].As<IDictionary<string,object>>();
        return new PostDTO
        {
            PostId = node["PostId"].As<string>(),
            PostedBy = node["PostedBy"].As<string>(),
            Timestamp = node["Timestamp"].As<long>(),
            Likes = node["Likes"].As<int>(),
            Content = node["Content"].As<string>(),
            MediaURL=node["MediaURL"].As<string>(),
            PostedByPic=node["PostedByPic"].As<string>(),
            Liked=node["Liked"].As<bool>(),
            PostedById=node["PostedById"].As<string>()
        };
    }

    public static List<PostDTO> ToPostList(List<IRecord> records,string nodeKey)
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
                ChatUser chatUser = new()
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
            Content =node["Content"].As<string>(),
            Thumbnail=node["Thumbnail"].As<string>()
            
        };
    }

    public static List<Notification> ToNotificationList(List<IRecord> records,string nodeKey)
    {
        return records.Select(record => ToNotification(record, nodeKey)).ToList();
    }
}