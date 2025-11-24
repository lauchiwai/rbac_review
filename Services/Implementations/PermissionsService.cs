using Common.DTOs;
using Common.Models;
using Repositories.MyRepository;
using Services.Interfaces;

namespace Services.Implementations;

public class PermissionsService : IPermissionsService
{
    private readonly IRepository<Permissions> _permissionRepository;

    public PermissionsService(IRepository<Permissions> permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<ResultDto<IEnumerable<Permissions>>> GetAllPermissionsAsync()
    {
        try
        {
            var permissions = await _permissionRepository.GetAllAsync();
            return ResultDto<IEnumerable<Permissions>>.Success(permissions);
        }
        catch (Exception ex)
        {
            return ResultDto<IEnumerable<Permissions>>.Failure($"Error retrieving permissions: {ex.Message}");
        }
    }

    public async Task<ResultDto<Permissions>> GetPermissionByIdAsync(int permissionId)
    {
        try
        {
            var permission = await _permissionRepository.GetByIdAsync(permissionId);
            if (permission == null)
            {
                return ResultDto<Permissions>.Failure($"Permission with ID {permissionId} not found");
            }
            return ResultDto<Permissions>.Success(permission);
        }
        catch (Exception ex)
        {
            return ResultDto<Permissions>.Failure($"Error retrieving permission: {ex.Message}");
        }
    }

    public async Task<ResultDto<Permissions>> CreatePermissionAsync(string permissionName)
    {
        try
        {
            var permission = new Permissions
            {
                PermissionName = permissionName
            };

            var createdPermission = await _permissionRepository.AddAsync(permission);
            return ResultDto<Permissions>.Success(createdPermission);
        }
        catch (Exception ex)
        {
            return ResultDto<Permissions>.Failure($"Error creating permission: {ex.Message}");
        }
    }

    public async Task<ResultDto> UpdatePermissionAsync(Permissions permission)
    {
        try
        {
            await _permissionRepository.UpdateAsync(permission);
            return ResultDto.Success();
        }
        catch (Exception ex)
        {
            return ResultDto.Failure($"Error updating permission: {ex.Message}");
        }
    }

    public async Task<ResultDto> DeletePermissionAsync(int permissionId)
    {
        try
        {
            var permission = await _permissionRepository.GetByIdAsync(permissionId);
            if (permission == null)
            {
                return ResultDto.Failure($"Permission with ID {permissionId} not found");
            }

            await _permissionRepository.DeleteAsync(permission);
            return ResultDto.Success();
        }
        catch (Exception ex)
        {
            return ResultDto.Failure($"Error deleting permission: {ex.Message}");
        }
    }
}