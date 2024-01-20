using System.Text.Json.Serialization;

namespace SocialNetworkApp.Server.Data.Entities
{
    public class UserDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = String.Empty;

        public string Email { get; set; } = String.Empty;

        public string Username { get; set; } = String.Empty;
        public string Bio { get; set; } = String.Empty;

        public string Thumbnail { get; set; } = "";

        public bool IsFriend { get; set; } = false;

        public bool SentRequest { get; set; } = false;

        public bool RecievedRequest { get; set; } = false;

    }
}
