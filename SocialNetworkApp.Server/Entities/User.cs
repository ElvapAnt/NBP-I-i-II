using System.Text.Json.Serialization;

namespace SocialNetworkApp.Server.Entities
{
    public class User
    {
        public string Id { get; set; } = $"userId:{Guid.NewGuid().ToString()}";
        public string Name { get; set; } = String.Empty;

        public string Email { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;

        public string Username { get; set; } = String.Empty;
        public string Bio { get; set; } = String.Empty;
    }
}
