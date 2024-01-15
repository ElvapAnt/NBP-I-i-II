namespace SocialNetworkApp.Server.Entities;

public class Message{
    public string MessageId { get; set; } = string.Empty;
    public string Content { get; set; } = String.Empty;
    public string Sender { get; set; } = String.Empty;
    public long Timestamp { get; set; } = 0;
}