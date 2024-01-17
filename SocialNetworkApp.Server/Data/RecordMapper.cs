using Neo4j.Driver;
using Neo4jClient;
using NRedisStack.Graph;
using SocialNetworkApp.Server.Data.Entities;

namespace SocialNetworkApp.Data;

public static class RecordMapper
{

    /// <summary>
    /// Thumbnail fali
    /// </summary>
    /// <param name="record"></param>
    /// <param name="nodeKey"></param>
    /// <returns></returns>
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
}