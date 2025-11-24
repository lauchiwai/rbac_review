using Common.DTOs;
using Common.DTOs.Rbac.Requests;
using Common.DTOs.Rbac.Responses;

namespace Services.Interfaces;

public interface IRbacService
{
    Task<ResultDto> AssignPermissionAsync(AssignPermissionRequest request);

    Task<ResultDto> RemovePermissionAsync(RemovePermissionRequest request);

    Task<ResultDto<RolePermissionsResponse>> GetRolePermissionsAsync(int roleId);

    Task<ResultDto<PermissionRolesResponse>> GetRolesByPermissionAsync(int permissionId);

    Task<ResultDto<PermissionCheckResponse>> CheckPermissionAsync(CheckPermissionRequest request);

    Task<ResultDto<bool>> HasPermissionAsync(int roleId, string permissionName);
}