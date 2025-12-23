using Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Repositories.MyRepository;

namespace Services.Helpers;

public interface IUserNameHelper
{
    Task<string> GetUserNameAsync(int userId);
    Task<Dictionary<int, string>> GetUserNamesAsync(List<int> userIds);
}

public class UserNameHelper : IUserNameHelper
{
    private readonly IRepository<Users> _userRepository;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

    public UserNameHelper(IRepository<Users> userRepository, IMemoryCache cache)
    {
        _userRepository = userRepository;
        _cache = cache;
    }

    public async Task<string> GetUserNameAsync(int userId)
    {
        var cacheKey = GetUserCacheKey(userId);

        if (_cache.TryGetValue(cacheKey, out string userName))
            return userName;

        var user = await _userRepository.GetByIdAsync(userId);
        userName = $"{user.Users_Roles} {user.UserId}";

        _cache.Set(cacheKey, userName, _cacheDuration);
        return userName;
    }

    public async Task<Dictionary<int, string>> GetUserNamesAsync(List<int> userIds)
    {
        if (!userIds.Any())
            return new Dictionary<int, string>();

        var result = new Dictionary<int, string>();
        var missingIds = new List<int>();

        // Get existing ones from cache
        foreach (var userId in userIds.Distinct())
        {
            var cacheKey = GetUserCacheKey(userId);
            if (_cache.TryGetValue(cacheKey, out string userName))
            {
                result[userId] = userName;
            }
            else
            {
                missingIds.Add(userId);
            }
        }

        // Batch query the missing ones
        if (missingIds.Any())
        {
            var users = await GetUsersByIdsAsync(missingIds);

            foreach (var user in users)
            {
                var userName = GetUserDisplayName(user);
                result[user.UserId] = userName;

                // Add to cache
                var cacheKey = GetUserCacheKey(user.UserId);
                _cache.Set(cacheKey, userName, _cacheDuration);
            }

            // Handle users not found
            var foundIds = users.Select(u => u.UserId).ToList();
            foreach (var missingId in missingIds.Where(id => !foundIds.Contains(id)))
            {
                var userName = GetDefaultUserName(missingId);
                result[missingId] = userName;

                // Also add to cache to avoid duplicate queries
                var cacheKey = GetUserCacheKey(missingId);
                _cache.Set(cacheKey, userName, _cacheDuration);
            }
        }

        return result;
    }

    private async Task<List<UserInfo>> GetUsersByIdsAsync(List<int> userIds)
    {
        return await _userRepository.GetQueryable()
            .Where(u => userIds.Contains(u.UserId))
            .Select(u => new UserInfo
            {
                UserId = u.UserId,
                RealName = $"{u.Users_Roles}{u.UserId}",
                Email = "",
                Username = $"{u.UserId}"
            })
            .ToListAsync();
    }

    private string GetUserDisplayName(UserInfo user)
    {
        if (user == null)
            return "Unknown User";

        return !string.IsNullOrEmpty(user.RealName)
            ? user.RealName
            : !string.IsNullOrEmpty(user.Username)
                ? user.Username
                : !string.IsNullOrEmpty(user.Email)
                    ? user.Email
                    : GetDefaultUserName(user.UserId);
    }

    private string GetDefaultUserName(int userId)
    {
        return $"User{userId}";
    }

    private string GetUserCacheKey(int userId)
    {
        return $"User_Name_{userId}";
    }

    // Internal class for storing user information
    private class UserInfo
    {
        public int UserId { get; set; }
        public string? RealName { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
    }
}