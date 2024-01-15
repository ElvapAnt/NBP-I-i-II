namespace SocialNetworkApp.Server.Entities;

public class Comment
{
    public string CommentId { get; set; } = string.Empty;
    public string Content { get; set; } = String.Empty;
    public long Timestamp { get; set; } = 0;
    public string PostedBy { get; set; } = String.Empty;
    public int Likes { get; set; } = 0;
    public string PostedByPic { get; set; } = String.Empty;
}