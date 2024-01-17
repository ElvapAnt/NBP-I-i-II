using Neo4j.Driver;
using Neo4jClient;
using NRedisStack.Graph;
using SocialNetworkApp.Server.Data.Entities;

namespace SocialNetworkApp.Data;

public static class RecordMapper
{

    //Not returning password here
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
            Password=node["Password"].As<string>()
        };
    }

    public static List<User> ToUserList(List<IRecord> records,string nodeKey)
    {
        return records.Select(record=>ToUser(record,nodeKey)
        ).ToList();
    }
}