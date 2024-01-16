using SocialNetworkApp.Server.Entities;
using SocialNetworkApp.Server.Repos;

namespace SocialNetworkApp.Services;

public class UserService(UserRepo repo)
{
    private readonly UserRepo _repo = repo;

    public async Task AddUser(User user)
    {
        await _repo.AddUser(user);
    }
}