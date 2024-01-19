namespace SocialNetworkApp.Server.Data.Entities;

public class Message{
    public string MessageId { get; set; } = string.Empty;
    public string Content { get; set; } = String.Empty;
    public string SenderId { get; set; } = String.Empty;
    public long Timestamp { get; set; } = 0;
    public bool Read { get; set; } = false;

    public bool Edited { get; set; } = false;
}