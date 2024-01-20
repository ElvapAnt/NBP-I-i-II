using SocialNetworkApp.Server.Data.Entities;
using SocialNetworkApp.Server.Business.Repos;
using SocialNetworkApp.Server.Error;
using Microsoft.AspNetCore.Identity;
using StackExchange.Redis;
using SocialNetworkApp.Server.Settings;
using Microsoft.AspNetCore.Mvc.Formatters;
using SocialNetworkApp.Server.Business.Services.Redis;

namespace SocialNetworkApp.Server.Business.Services;

public class UserService(UserRepo repo, ICacheService cacheService)
{
    private readonly UserRepo _repo = repo;
    private readonly PasswordHasher<User> _hasher = new();
    private readonly ICacheService _cacheService = cacheService;

    public async Task AddUser(User user)
    {
        var userExists = await _repo.GetUserByUsername(user.Username) != null;
        if (userExists)
            throw new CustomException("User with given username already exists.");
        user.Password = _hasher.HashPassword(user, user.Password);
        await _repo.AddUser(user);
        
        //mozda i ovde kesiranje cim se doda u bazu
        //cisto kao aktivni novi user npr
        var cacheKey = $"user:{user.UserId}";
        var cacheUser = new User
        {
            Username = user.Username,
            Bio = user.Bio,
            Thumbnail = user.Thumbnail
        };
        await _cacheService.SetCacheValueAsync(cacheKey, cacheUser, TimeSpan.FromMinutes(30));
    }

    public async Task<User> GetUser(string userId)
    {
        //provera da li postoji u kesu za brze prikupljanje
        var cacheKey = $"user:{userId}";
        var cachedUser = await _cacheService.GetCacheValueAsync<User>(cacheKey);
        if(cachedUser!=null)
            return cachedUser;

        //promasaj, pa se ide u bazu
        User user = await _repo.GetUser(userId) ?? throw new CustomException("No such user exists.");
        //poco da se ne bi slao password i email
        cachedUser = new User
        {
            Username = user.Username,
            Bio = user.Bio,
            Thumbnail = user.Thumbnail
        };
        await _cacheService.SetCacheValueAsync(cacheKey, cachedUser, TimeSpan.FromMinutes(30));
        return user;
    }

    public async Task<User> GetUserByUsername(string username)
    {
        var cacheKey = $"username:{username}";
        var cachedUser = await _cacheService.GetCacheValueAsync<User>(cacheKey);
        if (cachedUser != null)
            return cachedUser;

        User user = await _repo.GetUserByUsername(username) ?? throw new CustomException("No such user exists.");
        await _cacheService.SetCacheValueAsync(cacheKey, user, TimeSpan.FromMinutes(30));
        return user;
    }

    public async Task DeleteUser(string userId)
    {
        await _repo.DeleteUser(userId);

        //brisanje iz kesa
        var cacheKey = $"user:{userId}";
        await _cacheService.RemoveCacheValueAsync(cacheKey);
    }

    public async Task UpdateUsername(string userId,string newUsername)
    {
        User user =(await _repo.GetUser(userId))!;
        var oldUsername = user.Username;
        user.Username = newUsername;

        await _repo.UpdateUser(user);
        await _repo.UpdateUsersPosts(userId);
        await _repo.UpdateUsersChats(userId);
        await _repo.UpdateUsersNotifications(userId);

        //update cache
        //invalidira se stari username i updateuje na novi
        await _cacheService.RemoveCacheValueAsync($"username:{oldUsername}");

        var cacheKey = $"user:{userId}";
        var cacheUser = new User
        {
            Username = user.Username,
            Bio = user.Bio,
            Thumbnail = user.Thumbnail
        };
        
        await _cacheService.SetCacheValueAsync(cacheKey, cacheUser, TimeSpan.FromMinutes(30));
    }

   public async Task UpdateThumbnail(string userId,string newThumbnail)
    {
        User user = (await _repo.GetUser(userId))!;
        user.Thumbnail = newThumbnail;
        await _repo.UpdateUser(user);
        await _repo.UpdateUsersPosts(userId);
        await _repo.UpdateUsersChats(userId);

         await _cacheService.RemoveCacheValueAsync($"user:{userId}");

        //update cache isto kao za username 
        var cacheKey = $"user:{userId}";
        var cacheUser = new User
        {
            Username = user.Username,
            Bio = user.Bio,
            Thumbnail = user.Thumbnail
        };
        await _cacheService.SetCacheValueAsync(cacheKey, cacheUser, TimeSpan.FromMinutes(30));
    }

        //update cache isto kao za username 
        var cacheKey = $"user:{userId}";
        var cacheUser = new User
        {
            Username = user.Username,
            Bio = user.Bio,
            Thumbnail = user.Thumbnail
        };
        await _cacheService.SetCacheValueAsync(cacheKey, cacheUser, TimeSpan.FromMinutes(30));
    }

    public async Task<List<UserDTO>> GetFriends(string userId,int count,int skip)
    {
        var cacheKey= $"user:{userId}:friends";
        var cachedFriends = await _cacheService.GetListAsync<User>(cacheKey);
        //pogodak vraca kesiranu listu
        if (cachedFriends != null && cachedFriends.Any())
            return cachedFriends.Skip(skip).Take(count).ToList()!;

        //promasaj, pa se ide u bazu
        var friends = await _repo.GetFriends(userId, count, skip);
        await _cacheService.AddToListAsync(cacheKey, friends);
        return friends;
    }


    public async Task<User?> LogIn(string username,string password)
    {
        User? user = await GetUserByUsername(username);
        return _hasher.VerifyHashedPassword(user,user.Password,password)!=0?user:null;
    }

    //mozda i za ove funkcije kes da se ubaci
    public async Task<List<UserDTO>> GetRecommendedFriends(string userId,int count, int skip)
    {
        return await _repo.GetRecommendedFriends(userId, count, skip);
    }

    public async Task<List<UserDTO>> SearchForUsers(string usernamePattern,string userId)
    {
        return await _repo.SearchForUsers(usernamePattern, userId);
    }

    public async Task AddFriend(string userId1,string userId2)
    {
        await _repo.AddFriend(userId1, userId2);
    }

}