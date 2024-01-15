namespace SocialNetworkApp.Server.Settings
{
    public class NeoSettings : INeoSettings
    {
        public string Uri { get; set; } = String.Empty;
        public string User { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
    }
}
