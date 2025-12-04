using Common.Models;
using Repositories.MyRepository;
using System.Collections.Concurrent;

namespace Services.Helpers;

public class TodoQueryHelper
{
    private readonly IRepository<Users_Roles> _userRoleRepository;
    private readonly IRepository<Roles_Permissions> _rolePermissionRepository;
    private readonly IRepository<Permissions> _permissionRepository;
    private readonly IRepository<ReviewTemplates> _templateRepository;
    private readonly IRepository<ReviewStages> _stageRepository;
    private readonly IRepository<StageTransitions> _transitionRepository;
    private readonly IRepository<Users> _userRepository;
    private readonly IRepository<Roles> _roleRepository;

    public TodoQueryHelper(
        IRepository<Users_Roles> userRoleRepository,
        IRepository<Roles_Permissions> rolePermissionRepository,
        IRepository<Permissions> permissionRepository,
        IRepository<ReviewTemplates> templateRepository,
        IRepository<ReviewStages> stageRepository,
        IRepository<StageTransitions> transitionRepository,
        IRepository<Users> userRepository,
        IRepository<Roles> roleRepository)
    {
        _userRoleRepository = userRoleRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _permissionRepository = permissionRepository;
        _templateRepository = templateRepository;
        _stageRepository = stageRepository;
        _transitionRepository = transitionRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<Dictionary<int, bool>> CheckUsersPermissionsAsync(
        IEnumerable<int> userIds,
        string permissionName)
    {
        if (!userIds.Any()) return new Dictionary<int, bool>();

        // Batch retrieve user roles
        var userRoles = await _userRoleRepository.FindAsync(ur => userIds.Contains(ur.UserId));
        var userRoleDict = userRoles.GroupBy(ur => ur.UserId)
                                   .ToDictionary(g => g.Key, g => new HashSet<int>(g.Select(ur => ur.RoleId)));

        // Get permission ID
        var permissions = await _permissionRepository.FindAsync(p => p.PermissionName == permissionName);
        var permission = permissions.FirstOrDefault();
        if (permission == null)
            return userIds.ToDictionary(id => id, id => false);

        // Get role permissions
        var allRoleIds = userRoleDict.Values.SelectMany(v => v).Distinct().ToList();
        var rolePermissions = await _rolePermissionRepository.FindAsync(
            rp => allRoleIds.Contains(rp.RoleId) && rp.PermissionId == permission.PermissionId);

        // Build result
        var result = new ConcurrentDictionary<int, bool>();
        var validRoleIds = new HashSet<int>(rolePermissions.Select(rp => rp.RoleId));

        Parallel.ForEach(userIds, userId =>
        {
            var hasPermission = userRoleDict.TryGetValue(userId, out var roleIds) &&
                               roleIds.Any(roleId => validRoleIds.Contains(roleId));
            result[userId] = hasPermission;
        });

        return new Dictionary<int, bool>(result);
    }

    public async Task<bool> CheckUserPermissionAsync(int userId, string permissionName)
    {
        var result = await CheckUsersPermissionsAsync(new[] { userId }, permissionName);
        return result.TryGetValue(userId, out var hasPermission) && hasPermission;
    }

    public async Task<Dictionary<int, ReviewTemplates>> GetTemplatesByIdsAsync(IEnumerable<int> templateIds)
    {
        if (!templateIds.Any()) return new Dictionary<int, ReviewTemplates>();

        var templates = await _templateRepository.FindAsync(t => templateIds.Contains(t.TemplateId));
        return templates.ToDictionary(t => t.TemplateId);
    }

    public async Task<Dictionary<int, Users>> GetUsersByIdsAsync(IEnumerable<int> userIds)
    {
        if (!userIds.Any()) return new Dictionary<int, Users>();

        var users = await _userRepository.FindAsync(u => userIds.Contains(u.UserId));
        return users.ToDictionary(u => u.UserId);
    }

    public async Task<Dictionary<int, Roles>> GetRolesByIdsAsync(IEnumerable<int> roleIds)
    {
        if (!roleIds.Any()) return new Dictionary<int, Roles>();

        var roles = await _roleRepository.FindAsync(r => roleIds.Contains(r.RoleId));
        return roles.ToDictionary(r => r.RoleId);
    }

    public async Task<Dictionary<int, List<ReviewStages>>> GetStagesByTemplateIdsAsync(IEnumerable<int> templateIds)
    {
        if (!templateIds.Any()) return new Dictionary<int, List<ReviewStages>>();

        var stages = await _stageRepository.FindAsync(s => templateIds.Contains(s.TemplateId));
        return stages.GroupBy(s => s.TemplateId)
                    .ToDictionary(g => g.Key, g => g.OrderBy(s => s.StageOrder).ToList());
    }

    public async Task<Dictionary<int, ReviewStages>> GetFirstStagesByTemplateIdsAsync(IEnumerable<int> templateIds)
    {
        if (!templateIds.Any()) return new Dictionary<int, ReviewStages>();

        var stages = await _stageRepository.FindAsync(s =>
            templateIds.Contains(s.TemplateId) && s.StageOrder == 1);

        return stages.ToDictionary(s => s.TemplateId);
    }

    public async Task<Dictionary<int, bool>> CheckUsersExistAsync(IEnumerable<int> userIds)
    {
        if (!userIds.Any()) return new Dictionary<int, bool>();

        var users = await _userRepository.FindAsync(u => userIds.Contains(u.UserId));
        var existingUserIds = new HashSet<int>(users.Select(u => u.UserId));

        return userIds.Distinct().ToDictionary(id => id, id => existingUserIds.Contains(id));
    }

    public async Task<Dictionary<int, bool>> CheckRolesExistAsync(IEnumerable<int> roleIds)
    {
        if (!roleIds.Any()) return new Dictionary<int, bool>();

        var roles = await _roleRepository.FindAsync(r => roleIds.Contains(r.RoleId));
        var existingRoleIds = new HashSet<int>(roles.Select(r => r.RoleId));

        return roleIds.Distinct().ToDictionary(id => id, id => existingRoleIds.Contains(id));
    }

    public async Task<Dictionary<int, List<StageTransitions>>> GetTransitionsByTemplateIdsAsync(IEnumerable<int> templateIds)
    {
        if (!templateIds.Any()) return new Dictionary<int, List<StageTransitions>>();

        // First get all stages for the templates
        var stages = await _stageRepository.FindAsync(s => templateIds.Contains(s.TemplateId));
        var stageIds = stages.Select(s => s.StageId).ToList();

        if (!stageIds.Any()) return new Dictionary<int, List<StageTransitions>>();

        // Get transition rules for these stages
        var transitions = await _transitionRepository.FindAsync(t => stageIds.Contains(t.StageId));

        // Group by template
        var result = new Dictionary<int, List<StageTransitions>>();

        foreach (var stage in stages)
        {
            var stageTransitions = transitions.Where(t => t.StageId == stage.StageId).ToList();
            if (stageTransitions.Any())
            {
                if (!result.ContainsKey(stage.TemplateId))
                {
                    result[stage.TemplateId] = new List<StageTransitions>();
                }
                result[stage.TemplateId].AddRange(stageTransitions);
            }
        }

        return result;
    }

    public bool ValidateStageOrders(List<int> stageOrders)
    {
        if (!stageOrders.Any()) return false;

        var sortedOrders = stageOrders.OrderBy(o => o).ToList();
        var expectedOrders = Enumerable.Range(1, sortedOrders.Count).ToList();

        return sortedOrders.SequenceEqual(expectedOrders);
    }

    public string FormatUserName(int userId)
    {
        return $"User{userId}";
    }
}