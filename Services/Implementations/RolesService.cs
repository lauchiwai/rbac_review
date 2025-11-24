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
        private readonly IRoleRepository _roleRepository;

        public RolesService(IRepository<Roles> repository, IRoleRepository roleRepository)
        {
            _repository = repository;
            _roleRepository = roleRepository;
        }

        public async Task<ResultDto<IEnumerable<RoleListResponse>>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _roleRepository.GetAllRoleListResponseAsync();
                return ResultDto<IEnumerable<RoleListResponse>>.Success(roles);
            }
            catch (Exception ex)
            {
                return ResultDto<IEnumerable<RoleListResponse>>.Failure($"Error retrieving roles: {ex.Message}");
            }
        }

        public async Task<ResultDto<RoleResponse>> GetRoleByIdAsync(int roleId)
        {
            try
            {
                var role = await _roleRepository.GetRoleResponseByIdAsync(roleId);

                if (role == null)
                {
                    return ResultDto<RoleResponse>.Failure($"Role with ID {roleId} not found");
                }

                return ResultDto<RoleResponse>.Success(role);
            }
            catch (Exception ex)
            {
                return ResultDto<RoleResponse>.Failure($"Error retrieving role: {ex.Message}");
            }
        }

        public async Task<ResultDto<RoleResponse>> CreateRoleAsync(string roleName)
        {
            try
            {
                if (await _roleRepository.RoleExistsAsync(roleName))
                {
                    return ResultDto<RoleResponse>.Failure($"Role with name '{roleName}' already exists");
                }

                var role = new Roles
                {
                    RoleName = roleName
                };

                var createdRole = await _repository.AddAsync(role);

                var roleResponse = await _roleRepository.GetRoleResponseByIdAsync(createdRole.RoleId);
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
                if (await _roleRepository.RoleExistsAsync(role.RoleId, role.RoleName))
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