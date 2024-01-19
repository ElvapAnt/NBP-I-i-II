using SocialNetworkApp.Server.Data.Entities;
using SocialNetworkApp.Server.Business.Repos;
using SocialNetworkApp.Server.Error;
using Microsoft.AspNetCore.Identity;

namespace SocialNetworkApp.Server.Business.Services;

public class UserService(UserRepo repo)
{
    private readonly UserRepo _repo = repo;
    private readonly PasswordHasher<User> _hasher = new();
    public async Task AddUser(User user)
    {
        var userExists =await _repo.GetUserByUsername(user.Username) != null;
        if (userExists)
            throw new CustomException("User with given username already exists.");
        user.Password = _hasher.HashPassword(user, user.Password);
        await _repo.AddUser(user);
    }

    public async Task<User> GetUser(string userId)
    {
        User user = await _repo.GetUser(userId) ?? throw new CustomException("No such user exists.");
        return user;
    }

    public async Task<User> GetUserByUsername(string username)
    {
        User user = await _repo.GetUserByUsername(username) ?? throw new CustomException("No such user exists.");
        return user;
    }

    public async Task DeleteUser(string userId)
    {
        await _repo.DeleteUser(userId);
    }

    public async Task UpdateUsername(string userId,string newUsername)
    {
        User user =(await _repo.GetUser(userId))!;
        user.Username = newUsername;
        await _repo.UpdateUser(user);
        await _repo.UpdateUsersPosts(userId);
        await _repo.UpdateUsersChats(userId);
        await _repo.UpdateUsersNotifications(userId);
    }

    public async Task UpdateThumbnail(string userId,string newThumbnail)
    {
        User user = (await _repo.GetUser(userId))!;
        user.Thumbnail = newThumbnail;
        await _repo.UpdateUser(user);
        await _repo.UpdateUsersPosts(userId);
        await _repo.UpdateUsersChats(userId);
    }

    public async Task<List<User>> GetFriends(string userId,int count,int skip)
    {
        return await _repo.GetFriends(userId, count, skip);
    }

    public async Task<User?> LogIn(string username,string password)
    {
        User? user = await GetUserByUsername(username);
        return _hasher.VerifyHashedPassword(user,user.Password,password)!=0?user:null;
    }

    public async Task<List<User>> GetRecommendedFriends(string userId,int count, int skip)
    {
        return await _repo.GetRecommendedFriends(userId, count, skip);
    }

    public async Task<List<User>> SearchForUsers(string usernamePattern)
    {
        return await _repo.SearchForUsers(usernamePattern);
    }
}