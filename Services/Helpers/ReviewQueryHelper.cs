using Common.Models;
using Repositories.MyRepository;
using System.Collections.Concurrent;

namespace Services.Helpers;

public class ReviewQueryHelper
{
    private readonly IRepository<Users_Roles> _userRoleRepository;
    private readonly IRepository<Roles_Permissions> _rolePermissionRepository;
    private readonly IRepository<ReviewStages> _stageRepository;
    private readonly IRepository<Users> _userRepository;
    private readonly IRepository<Roles> _roleRepository;
    private readonly IRepository<StageTransitions> _transitionRepository;
    private readonly IRepository<Permissions> _permissionRepository;

    public ReviewQueryHelper(
        IRepository<Users_Roles> userRoleRepository,
        IRepository<Roles_Permissions> rolePermissionRepository,
        IRepository<ReviewStages> stageRepository,
        IRepository<Users> userRepository,
        IRepository<Roles> roleRepository,
        IRepository<StageTransitions> transitionRepository,
        IRepository<Permissions> permissionRepository)
    {
        _userRoleRepository = userRoleRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _stageRepository = stageRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _transitionRepository = transitionRepository;
        _permissionRepository = permissionRepository;
    }

    // Batch check user role permissions
    public async Task<HashSet<int>> GetUserRolesAsync(int userId)
    {
        var userRoles = await _userRoleRepository.FindAsync(ur => ur.UserId == userId);
        return new HashSet<int>(userRoles.Select(ur => ur.RoleId));
    }

    // Batch check role permissions for multiple users
    public async Task<Dictionary<int, HashSet<int>>> GetUsersRolesAsync(IEnumerable<int> userIds)
    {
        if (!userIds.Any()) return new Dictionary<int, HashSet<int>>();

        var userRoles = await _userRoleRepository.FindAsync(ur => userIds.Contains(ur.UserId));

        var result = new ConcurrentDictionary<int, HashSet<int>>();

        var groups = userRoles.GroupBy(ur => ur.UserId);

        Parallel.ForEach(groups, group =>
        {
            result[group.Key] = new HashSet<int>(group.Select(g => g.RoleId));
        });

        return new Dictionary<int, HashSet<int>>(result);
    }

    // Batch retrieve stage information
    public async Task<Dictionary<int, ReviewStages>> GetStagesByIdsAsync(IEnumerable<int> stageIds)
    {
        if (!stageIds.Any()) return new Dictionary<int, ReviewStages>();

        var stages = await _stageRepository.FindAsync(s => stageIds.Contains(s.StageId));
        return stages.ToDictionary(s => s.StageId);
    }

    // Batch retrieve all stages for templates
    public async Task<Dictionary<int, List<ReviewStages>>> GetStagesByTemplateIdsAsync(IEnumerable<int> templateIds)
    {
        if (!templateIds.Any()) return new Dictionary<int, List<ReviewStages>>();

        var stages = await _stageRepository.FindAsync(s => templateIds.Contains(s.TemplateId));
        return stages.GroupBy(s => s.TemplateId)
                    .ToDictionary(g => g.Key, g => g.ToList());
    }

    // Batch retrieve transition rules
    public async Task<Dictionary<int, List<StageTransitions>>> GetTransitionsByStageIdsAsync(IEnumerable<int> stageIds)
    {
        if (!stageIds.Any()) return new Dictionary<int, List<StageTransitions>>();

        var transitions = await _transitionRepository.FindAsync(t => stageIds.Contains(t.StageId));
        return transitions.GroupBy(t => t.StageId)
                         .ToDictionary(g => g.Key, g => g.ToList());
    }

    // Batch retrieve transition rules for specific actions
    public async Task<Dictionary<int, Dictionary<string, StageTransitions>>> GetTransitionsByStageIdsWithActionsAsync(IEnumerable<int> stageIds)
    {
        if (!stageIds.Any()) return new Dictionary<int, Dictionary<string, StageTransitions>>();

        var transitions = await _transitionRepository.FindAsync(t => stageIds.Contains(t.StageId));

        var result = new Dictionary<int, Dictionary<string, StageTransitions>>();

        foreach (var group in transitions.GroupBy(t => t.StageId))
        {
            var actionDict = group.ToDictionary(t => t.ActionName);
            result[group.Key] = actionDict;
        }

        return result;
    }

    // Batch retrieve user information
    public async Task<Dictionary<int, Users>> GetUsersByIdsAsync(IEnumerable<int> userIds)
    {
        if (!userIds.Any()) return new Dictionary<int, Users>();

        var users = await _userRepository.FindAsync(u => userIds.Contains(u.UserId));
        return users.ToDictionary(u => u.UserId);
    }

    // Batch retrieve role information
    public async Task<Dictionary<int, Roles>> GetRolesByIdsAsync(IEnumerable<int> roleIds)
    {
        if (!roleIds.Any()) return new Dictionary<int, Roles>();

        var roles = await _roleRepository.FindAsync(r => roleIds.Contains(r.RoleId));
        return roles.ToDictionary(r => r.RoleId);
    }

    // Check if user has specific permission
    public async Task<bool> UserHasPermissionAsync(int userId, string permissionName)
    {
        try
        {
            // Get all roles for user
            var userRoles = await GetUserRolesAsync(userId);
            if (!userRoles.Any()) return false;

            // Get permission ID
            var permissions = await _permissionRepository.FindAsync(p => p.PermissionName == permissionName);
            var permission = permissions.FirstOrDefault();
            if (permission == null) return false;

            // Check if user has this permission
            var rolePermissions = await _rolePermissionRepository.FindAsync(
                rp => userRoles.Contains(rp.RoleId) && rp.PermissionId == permission.PermissionId);

            return rolePermissions.Any();
        }
        catch
        {
            return false;
        }
    }

    // Batch retrieve user permissions - Added this missing method
    public async Task<Dictionary<int, HashSet<string>>> GetUsersPermissionsAsync(IEnumerable<int> userIds)
    {
        if (!userIds.Any()) return new Dictionary<int, HashSet<string>>();

        // Get all roles for users
        var userRoles = await _userRoleRepository.FindAsync(ur => userIds.Contains(ur.UserId));
        var userRoleDict = userRoles.GroupBy(ur => ur.UserId)
                                   .ToDictionary(g => g.Key, g => new HashSet<int>(g.Select(ur => ur.RoleId)));

        // Get all permissions for roles
        var allRoleIds = userRoleDict.Values.SelectMany(v => v).Distinct().ToList();
        var rolePermissions = await _rolePermissionRepository.FindAsync(rp => allRoleIds.Contains(rp.RoleId));

        // Get all permission names
        var permissionIds = rolePermissions.Select(rp => rp.PermissionId).Distinct().ToList();
        var permissions = await _permissionRepository.FindAsync(p => permissionIds.Contains(p.PermissionId));
        var permissionDict = permissions.ToDictionary(p => p.PermissionId, p => p.PermissionName);

        // Build result
        var result = new Dictionary<int, HashSet<string>>();

        foreach (var userId in userIds)
        {
            var userPerms = new HashSet<string>();

            if (userRoleDict.TryGetValue(userId, out var roleIds))
            {
                foreach (var roleId in roleIds)
                {
                    var rolePerms = rolePermissions.Where(rp => rp.RoleId == roleId);
                    foreach (var rp in rolePerms)
                    {
                        if (permissionDict.TryGetValue(rp.PermissionId, out var permName))
                        {
                            userPerms.Add(permName);
                        }
                    }
                }
            }

            result[userId] = userPerms;
        }

        return result;
    }

    // Get single user permissions - Added this helper method
    public async Task<HashSet<string>> GetUserPermissionsAsync(int userId)
    {
        var permissionsDict = await GetUsersPermissionsAsync(new[] { userId });
        return permissionsDict.TryGetValue(userId, out var permissions)
            ? permissions
            : new HashSet<string>();
    }

    // Format user display name
    public string FormatUserName(int userId)
    {
        return $"User{userId}";
    }

    // Batch format user display names
    public Dictionary<int, string> FormatUserNames(IEnumerable<int> userIds)
    {
        return userIds.Distinct().ToDictionary(id => id, id => $"User{id}");
    }

    // Check if user has required role for stage
    public async Task<bool> UserHasRequiredRoleAsync(int userId, int requiredRoleId)
    {
        var userRoles = await GetUserRolesAsync(userId);
        return userRoles.Contains(requiredRoleId);
    }

    public async Task<Dictionary<int, bool>> CheckUsersRolesAsync(IEnumerable<int> userIds, IEnumerable<int> requiredRoleIds)
    {
        if (!userIds.Any() || !requiredRoleIds.Any())
            return userIds.ToDictionary(id => id, id => false);

        var usersRolesDict = await GetUsersRolesAsync(userIds);
        var result = new Dictionary<int, bool>();

        foreach (var userId in userIds)
        {
            if (usersRolesDict.TryGetValue(userId, out var userRoles))
            {
                result[userId] = requiredRoleIds.Any(roleId => userRoles.Contains(roleId));
            }
            else
            {
                result[userId] = false;
            }
        }

        return result;
    }
}