namespace SocialNetworkApp.Server.Settings
{
    public interface INeoSettings
    {
        string Uri { get; set; }
        string User { get; set; }
        string Password { get; set; }
    }
}
