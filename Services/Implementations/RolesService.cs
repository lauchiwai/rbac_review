using Common.DTOs;
using Common.DTOs.Roles.Responses;
using Common.Models;
using Repositories.MyRepository;
using Services.Interfaces;

namespace Services.Implementations
{
    public class RolesService : IRolesService
    {
        private readonly IRepository<Roles> _repository;


        public RolesService(
            IRepository<Roles> repository)
        {
            _repository = repository;
        }

        public async Task<ResultDto<IEnumerable<RoleListResponse>>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _repository.FindWithIncludesAsync(null, r => r.Roles_Permissions);

                var roleResponses = roles.Select(r => new RoleListResponse
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    PermissionCount = r.Roles_Permissions.Count
                }).ToList();

                return ResultDto<IEnumerable<RoleListResponse>>.Success(roleResponses);
            }
            catch (Exception ex)
            {
                return ResultDto<IEnumerable<RoleListResponse>>.Failure($"Error retrieving roles: {ex.Message}");
            }
        }

        public async Task<ResultDto<RoleResponse>> CreateRoleAsync(string roleName)
        {
            try
            {
                var existingRole = await _repository.FindAsync(r => r.RoleName == roleName);
                if (existingRole.Any())
                {
                    return ResultDto<RoleResponse>.Failure($"Role with name '{roleName}' already exists");
                }

                var role = new Roles
                {
                    RoleName = roleName
                };

                var createdRole = await _repository.AddAsync(role);

                var roleResponse = new RoleResponse
                {
                    RoleId = createdRole.RoleId,
                    RoleName = createdRole.RoleName,
                    RolePermissions = new List<RolePermissionResponse>()
                };

                return ResultDto<RoleResponse>.Success(roleResponse);
            }
            catch (Exception ex)
            {
                return ResultDto<RoleResponse>.Failure($"Error creating role: {ex.Message}");
            }
        }

        public async Task<ResultDto> UpdateRoleAsync(Roles role)
        {
            try
            {
                var existingRoles = await _repository.FindAsync(r =>
                    r.RoleName == role.RoleName && r.RoleId != role.RoleId);

                if (existingRoles.Any())
                {
                    return ResultDto.Failure($"Role with name '{role.RoleName}' already exists");
                }

                await _repository.UpdateAsync(role);
                return ResultDto.Success();
            }
            catch (Exception ex)
            {
                return ResultDto.Failure($"Error updating role: {ex.Message}");
            }
        }

        public async Task<ResultDto> DeleteRoleAsync(int roleId)
        {
            try
            {
                var role = await _repository.GetByIdAsync(roleId);
                if (role == null)
                {
                    return ResultDto.Failure($"Role with ID {roleId} not found");
                }

                await _repository.DeleteAsync(role);
                return ResultDto.Success();
            }
            catch (Exception ex)
            {
                return ResultDto.Failure($"Error deleting role: {ex.Message}");
            }
        }
    }
}