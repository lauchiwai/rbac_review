using Common.DTOs;
using Common.DTOs.Roles.Responses;
using Common.Models;

namespace Services.Interfaces
{
    public interface IRolesService
    {
        Task<ResultDto<IEnumerable<RoleListResponse>>> GetAllRolesAsync();

        Task<ResultDto<RoleResponse>> GetRoleByIdAsync(int roleId);

        Task<ResultDto<RoleResponse>> CreateRoleAsync(string roleName);

        Task<ResultDto> UpdateRoleAsync(Roles role);

        Task<ResultDto> DeleteRoleAsync(int roleId);
    }
}