using SocialNetworkApp.Server.Data.Entities;
using SocialNetworkApp.Server.Business.Repos;
using SocialNetworkApp.Server.Error;
using Microsoft.AspNetCore.Identity;
using SocialNetworkApp.Server.Business.Services.Redis;
using Newtonsoft.Json;

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
    }

    public async Task<User> GetUser(string userId)
    {
        return await _repo.GetUser(userId)??throw new CustomException("No such user exists.");
    }

    public async Task<UserDTO> GetUserDTO(string userId,string userId2)
    {
        string userCacheKey = $"profile:{userId}";
        var userToUserCacheKey = $"{userId}++{userId2}";
        UserDTO userDto;
        if(_cacheService.KeyExists(userCacheKey)&&_cacheService.KeyExists(userToUserCacheKey))
        {
            userDto= await _cacheService.GetHashSet<UserDTO>(userCacheKey);
            var specificUserDto = await _cacheService.GetHashSet<UserDTO>(userToUserCacheKey);
            userDto.IsFriend = specificUserDto.IsFriend;
            userDto.SentRequest=specificUserDto.SentRequest;
            userDto.RecievedRequest = specificUserDto.RecievedRequest;
        }
        else
        {
            userDto = await _repo.GetUserDTO(userId,userId2) ?? throw new CustomException("No such user exists.");
            await _cacheService.CreateHashSetFrom<UserDTO>(userCacheKey, userDto,TimeSpan.FromMinutes(2));
            await _cacheService.UpdateHashSet(userToUserCacheKey, [
                new("IsFriend",JsonConvert.SerializeObject(userDto.IsFriend))
                ,new("SentRequest",JsonConvert.SerializeObject(userDto.SentRequest))
                ,new("RecievedRequest",JsonConvert.SerializeObject(userDto.RecievedRequest))
            ],TimeSpan.FromMinutes(2));
        }

       /*  var cachedUser = await _cacheService.GetCacheValueAsync<UserDTO>(userToUserCacheKey);
        if(cachedUser!=null)
            return cachedUser;
        //promasaj, pa se ide u bazu
        UserDTO userDto = await _repo.GetUserDTO(userId,userId2) ?? throw new CustomException("No such user exists.");
        await _cacheService.SetCacheValueAsync(userToUserCacheKey, userDto, TimeSpan.FromMinutes(2)); */
        return userDto;
    }

    public async Task<User> GetUserByUsername(string username)
    {
       /*  var cacheKey = $"username:{username}";
        var cachedUser = await _cacheService.GetCacheValueAsync<User>(cacheKey);
        if (cachedUser != null)
            return cachedUser;
 */
        User user = await _repo.GetUserByUsername(username) ?? throw new CustomException("No such user exists.");
     /*    await _cacheService.SetCacheValueAsync(cacheKey, user, TimeSpan.FromMinutes(60)); */
        return user;
    }

    public async Task DeleteUser(string userId)
    {
        await _repo.DeleteUser(userId);

    }

    public async Task UpdateUsername(string userId,string newUsername)
    {
        User user = (await _repo.GetUser(userId))!;
        user.Username = newUsername;

        await _repo.UpdateUser(user);
        await _repo.UpdateUsersPosts(userId);
        await _repo.UpdateUsersChats(userId);
        await _repo.UpdateUsersNotifications(userId);


        await _cacheService.UpdateHashSet($"profile:{userId}", [
            new("Username",JsonConvert.SerializeObject(newUsername))
        ],TimeSpan.FromMinutes(2));

    }

   public async Task UpdateThumbnail(string userId,string newThumbnail)
    {
        User user = (await _repo.GetUser(userId))!;
        user.Thumbnail = newThumbnail;
        await _repo.UpdateUser(user);
        await _repo.UpdateUsersPosts(userId);
        await _repo.UpdateUsersChats(userId);
        await _repo.UpdateUsersNotifications(userId);

        
        //await _cacheService.SetCacheValueAsync($"{userId}++{userId}", user, TimeSpan.FromMinutes(2));
              await _cacheService.UpdateHashSet($"profile:{userId}", [
            new("Thumbnail",JsonConvert.SerializeObject(newThumbnail))
        ],TimeSpan.FromMinutes(2));
    }


    public async Task<List<UserDTO>> GetFriends(string userId,int count=0x7FFFFFFF,int skip=0)
    {
        var cacheKey= $"{userId}:friends";
        var cachedFriends = await _cacheService.GetListAsync<UserDTO>(cacheKey);
        //pogodak vraca kesiranu listu
        if (cachedFriends != null && cachedFriends.Any())
            return cachedFriends.Skip(skip).Take(count).ToList()!;

        //promasaj, pa se ide u bazu
        var friends = await _repo.GetFriends(userId, count, skip);
        await _cacheService.AddToListFrom(cacheKey, friends,TimeSpan.FromMinutes(2));
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
        if(_cacheService.KeyExists(usernamePattern))
        {
            return await _cacheService.GetCacheValueAsync<List<UserDTO>>(usernamePattern);
        }
        var users =await _repo.SearchForUsers(usernamePattern, userId);
        await _cacheService.SetCacheValueAsync<List<UserDTO>>(usernamePattern,users,TimeSpan.FromMinutes(3));
        return users;
    }

    public async Task AddFriend(string userId1,string userId2)
    {
        await _repo.AddFriend(userId1, userId2);
        string cacheKey = $"{userId1}:friends";
        UserDTO user;

        if(_cacheService.KeyExists(cacheKey))
        {
            user = await GetUserDTO(userId2, userId1);
            await _cacheService.AddToListHeadAsync(cacheKey, user, TimeSpan.FromMinutes(2));
        }

        cacheKey = $"{userId2}:friends";
        if(_cacheService.KeyExists(cacheKey))
        {
            user = await GetUserDTO(userId1, userId2);
            await _cacheService.AddToListHeadAsync(cacheKey, user, TimeSpan.FromMinutes(2));
        }
    }

}