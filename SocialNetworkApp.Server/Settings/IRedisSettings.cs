namespace SocialNetworkApp.Server.Settings
{
    public interface IRedisSettings
    {
        string ConnectionString { get; set; }
        string InstanceName { get; set; }
    }
}
