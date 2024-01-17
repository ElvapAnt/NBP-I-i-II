using SocialNetworkApp.Server.Data.Entities;
using SocialNetworkApp.Server.Business.Repos;

namespace SocialNetworkApp.Business.Services;

public class UserService(UserRepo repo)
{
    private readonly UserRepo _repo = repo;

    public async Task AddUser(User user)
    {
        await _repo.AddUser(user);
    }

    public async Task<User> GetUser(string userId)
    {
        User user = await _repo.GetUser(userId) ?? throw new Exception("No such user exists.");
        return user;
    }

    public async Task<User> GetUserByUsername(string username)
    {
        User user = await _repo.GetUserByUsername(username) ?? throw new Exception("No such user exists.");
        return user;
    }

    public async Task DeleteUser(string userId)
    {
        await _repo.DeleteUser(userId);
    }

    public async Task UpdateUser(User user)
    {
        await _repo.UpdateUser(user);
    }

    public async Task<List<User>> GetFriends(string userId,int count,int skip)
    {
        return await _repo.GetFriends(userId, count, skip);
    }

    public async Task<bool> LogIn(string username,string password)
    {
        var user = await GetUserByUsername(username);
        return user.Password == password;
    }

    public async Task<List<User>> GetRecommendedFriends(string userId,int count, int skip)
    {
        return await _repo.GetRecommendedFriends(userId, count, skip);
    }
}