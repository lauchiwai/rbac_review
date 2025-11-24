using Common.DTOs;
using Common.Models;

namespace Services.Interfaces;

public interface IPermissionsService
{
    Task<ResultDto<IEnumerable<Permissions>>> GetAllPermissionsAsync();

    Task<ResultDto<Permissions>> GetPermissionByIdAsync(int permissionId);

    Task<ResultDto<Permissions>> CreatePermissionAsync(string permissionName);

    Task<ResultDto> UpdatePermissionAsync(Permissions permission);

    Task<ResultDto> DeletePermissionAsync(int permissionId);
}