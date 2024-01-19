

namespace SocialNetworkApp.Server.Data.Entities;

public class ChatUser
{
    public string Username { get; set; } = "";
    public string Thumbnail { get; set; } = "";
}
public class Chat
{
    public string ChatId { get; set; } = String.Empty;
    public string Name { get; set; } = String.Empty;
    public long CreationTimestamp { get; set; } = 0;
    public long LatestTimestamp { get; set; } = 0;
    public Dictionary<string,ChatUser> Members { get; set; } = [];
}