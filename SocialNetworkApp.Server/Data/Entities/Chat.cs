namespace SocialNetworkApp.Server.Data.Entities;

public class Chat
{
    public string ChatId { get; set; } = String.Empty;
    public string Name { get; set; } = String.Empty;
    public long CreationTimestamp { get; set; } = 0;
}