using Neo4j.Driver;
using SocialNetworkApp.Server.Entities;

namespace SocialNetworkApp.Server.Repos;

public class UserRepo(IDriver driver)
{
    private readonly IDriver _driver = driver;

    public async Task AddUser(User user)
    {
        user.UserId = Guid.NewGuid().ToString();
        using var session = _driver.AsyncSession();
        string query = "CREATE (u: User $user) RETURN u";
        var parameters = new { user };

        await session.RunAsync(query, parameters);
    }
}