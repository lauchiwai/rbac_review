using Common.DTOs;
using Common.DTOs.Rbac.Requests;
using Common.DTOs.Rbac.Responses;
using Common.Models;
using Repositories.MyRepository;
using Services.Interfaces;
using System.Linq.Expressions;

namespace Services.Implementations;

public class RbacService : IRbacService
{
    private readonly IRepository<Roles_Permissions> _rolePermissionRepository;
    private readonly IRepository<Roles> _roleRepository;
    private readonly IRepository<Permissions> _permissionRepository;

    public RbacService(
        IRepository<Roles_Permissions> rolePermissionRepository,
        IRepository<Roles> roleRepository,
        IRepository<Permissions> permissionRepository)
    {
        _rolePermissionRepository = rolePermissionRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
    }

    public async Task<ResultDto> AssignPermissionAsync(AssignPermissionRequest request)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(request.RoleId);
            var permission = await _permissionRepository.GetByIdAsync(request.PermissionId);

            if (role == null)
                return ResultDto.Failure($"Role with ID {request.RoleId} not found");

            if (permission == null)
                return ResultDto.Failure($"Permission with ID {request.PermissionId} not found");

            Expression<Func<Roles_Permissions, bool>> predicate = rp =>
                rp.RoleId == request.RoleId && rp.PermissionId == request.PermissionId;

            var existing = await _rolePermissionRepository.FindAsync(predicate);
            if (existing.Any())
                return ResultDto.Failure("Permission is already assigned to this role");

            var rolePermission = new Roles_Permissions
            {
                RoleId = request.RoleId,
                PermissionId = request.PermissionId
            };

            await _rolePermissionRepository.AddAsync(rolePermission);
            return ResultDto.Success("Permission assigned to role successfully");
        }
        catch (Exception ex)
        {
            return ResultDto.Failure($"Error assigning permission to role: {ex.Message}");
        }
    }

    public async Task<ResultDto> RemovePermissionAsync(RemovePermissionRequest request)
    {
        try
        {
            Expression<Func<Roles_Permissions, bool>> predicate = rp =>
                rp.RoleId == request.RoleId && rp.PermissionId == request.PermissionId;

            var rolePermissions = await _rolePermissionRepository.FindAsync(predicate);
            var rolePermission = rolePermissions.FirstOrDefault();

            if (rolePermission == null)
                return ResultDto.Failure("Permission is not assigned to this role");

            await _rolePermissionRepository.DeleteAsync(rolePermission);
            return ResultDto.Success("Permission removed from role successfully");
        }
        catch (Exception ex)
        {
            return ResultDto.Failure($"Error removing permission: {ex.Message}");
        }
    }

    public async Task<ResultDto<RolePermissionsResponse>> GetRolePermissionsAsync(int roleId)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
                return ResultDto<RolePermissionsResponse>.Failure($"Role with ID {roleId} not found");

            Expression<Func<Roles_Permissions, bool>> predicate = rp => rp.RoleId == roleId;
            var rolePermissions = await _rolePermissionRepository.FindAsync(predicate);

            var permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToList();
            var permissions = new List<PermissionViewModel>();

            foreach (var permissionId in permissionIds)
            {
                var permission = await _permissionRepository.GetByIdAsync(permissionId);
                if (permission != null)
                {
                    permissions.Add(new PermissionViewModel
                    {
                        PermissionId = permission.PermissionId,
                        PermissionName = permission.PermissionName
                    });
                }
            }

            var response = new RolePermissionsResponse
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Permissions = permissions
            };

            return ResultDto<RolePermissionsResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return ResultDto<RolePermissionsResponse>.Failure($"Error retrieving role permissions: {ex.Message}");
        }
    }

    public async Task<ResultDto<PermissionRolesResponse>> GetRolesByPermissionAsync(int permissionId)
    {
        try
        {
            var permission = await _permissionRepository.GetByIdAsync(permissionId);
            if (permission == null)
                return ResultDto<PermissionRolesResponse>.Failure($"Permission with ID {permissionId} not found");

            Expression<Func<Roles_Permissions, bool>> predicate = rp => rp.PermissionId == permissionId;
            var rolePermissions = await _rolePermissionRepository.FindAsync(predicate);

            var roleIds = rolePermissions.Select(rp => rp.RoleId).ToList();
            var roles = new List<RoleViewModel>();

            foreach (var roleId in roleIds)
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role != null)
                {
                    roles.Add(new RoleViewModel
                    {
                        RoleId = role.RoleId,
                        RoleName = role.RoleName
                    });
                }
            }

            var response = new PermissionRolesResponse
            {
                PermissionId = permission.PermissionId,
                PermissionName = permission.PermissionName,
                Roles = roles
            };

            return ResultDto<PermissionRolesResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return ResultDto<PermissionRolesResponse>.Failure($"Error retrieving roles by permission: {ex.Message}");
        }
    }

    public async Task<ResultDto<PermissionCheckResponse>> CheckPermissionAsync(CheckPermissionRequest request)
    {
        try
        {
            var hasPermission = await HasPermissionAsync(request.RoleId, request.PermissionName);

            var response = new PermissionCheckResponse
            {
                RoleId = request.RoleId,
                PermissionName = request.PermissionName,
                HasPermission = hasPermission.Data,
                Message = hasPermission.Data ? "Permission granted" : "Permission denied"
            };

            return ResultDto<PermissionCheckResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return ResultDto<PermissionCheckResponse>.Failure($"Error checking permission: {ex.Message}");
        }
    }

    public async Task<ResultDto<bool>> HasPermissionAsync(int roleId, string permissionName)
    {
        try
        {
            Expression<Func<Permissions, bool>> permissionPredicate = p => p.PermissionName == permissionName;
            var permissions = await _permissionRepository.FindAsync(permissionPredicate);
            var permission = permissions.FirstOrDefault();

            if (permission == null)
                return ResultDto<bool>.Success(false);

            Expression<Func<Roles_Permissions, bool>> rolePermissionPredicate = rp =>
                rp.RoleId == roleId && rp.PermissionId == permission.PermissionId;

            var rolePermissions = await _rolePermissionRepository.FindAsync(rolePermissionPredicate);
            var hasPermission = rolePermissions.Any();

            return ResultDto<bool>.Success(hasPermission);
        }
        catch (Exception ex)
        {
            return ResultDto<bool>.Failure($"Error checking permission: {ex.Message}");
        }
    }
}