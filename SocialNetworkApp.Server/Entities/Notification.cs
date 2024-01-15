namespace SocialNetworkApp.Server.Entities;

public class Notification
{
    public string NotificationId { get; set; } = string.Empty;
    public long Timestamp{ get; set; }
    public NotificationType Type{ get; set;}

    public string Content { get; set; } = string.Empty;
    public string URL { get; set; } = string.Empty;

}

public enum NotificationType
{
    LIKE,COMMENT,REQUEST,INVITE
}