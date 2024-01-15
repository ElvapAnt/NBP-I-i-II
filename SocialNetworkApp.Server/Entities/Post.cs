namespace SocialNetworkApp.Server.Entities;

public class Post{
    public string PostId { get; set; } = string.Empty;
    public string PostedBy { get; set; } = String.Empty;
    public int Likes { get; set; } = 0;



}