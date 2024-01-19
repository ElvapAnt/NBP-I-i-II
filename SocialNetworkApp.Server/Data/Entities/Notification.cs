namespace SocialNetworkApp.Server.Data.Entities;

public class Notification
{
    public string NotificationId { get; set; } = string.Empty;
    public long Timestamp{ get; set; }
    public string Content { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;
    public bool Viewed { get; set; } = false;

    public string From { get; set; } = "";

}
