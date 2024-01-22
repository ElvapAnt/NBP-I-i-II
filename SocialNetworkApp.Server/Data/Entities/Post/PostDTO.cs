namespace SocialNetworkApp.Server.Data.Entities;

public class PostDTO
{
    public string PostId { get; set; } = string.Empty;
    public string PostedBy { get; set; } = string.Empty;
    public int Likes { get; set; } = 0;

    public string Content { get; set; } = string.Empty;

    public string MediaURL { get; set; } = string.Empty;

    public long Timestamp { get; set; } = 0;

    public string PostedByPic { get; set; } = String.Empty;

    public bool Liked{ get; set; }
    public string PostedById { get; set; } = String.Empty;
}