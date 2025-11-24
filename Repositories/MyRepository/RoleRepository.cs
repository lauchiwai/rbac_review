using Common.DTOs.Roles.Responses;
using Common.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.MyDbContext;

namespace Repositories.MyRepository
{
    public interface IRoleRepository : IRepository<Roles>
    {
        Task<bool> RoleExistsAsync(string roleName);

        Task<bool> RoleExistsAsync(int roleId, string roleName);

        Task<IEnumerable<RoleListResponse>> GetAllRoleListResponseAsync();

        Task<RoleResponse> GetRoleResponseByIdAsync(int roleId);

    }

    public class RoleRepository : Repository<Roles>, IRoleRepository
    {
        private readonly Context _context;

        public RoleRepository(Context context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _context.Roles
                .AnyAsync(r => r.RoleName == roleName);
        }

        public async Task<bool> RoleExistsAsync(int roleId, string roleName)
        {
            return await _context.Roles
                .AnyAsync(r => r.RoleId != roleId && r.RoleName == roleName);
        }

        public async Task<IEnumerable<RoleListResponse>> GetAllRoleListResponseAsync()
        {
            return await _context.Roles
                .Include(r => r.Roles_Permissions)
                .Select(role => new RoleListResponse
                {
                    RoleId = role.RoleId,
                    RoleName = role.RoleName,
                    PermissionCount = role.Roles_Permissions.Count
                })
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<RoleResponse> GetRoleResponseByIdAsync(int roleId)
        {
            return await _context.Roles
                .Where(r => r.RoleId == roleId)
                .Include(r => r.Roles_Permissions)
                    .ThenInclude(rp => rp.Permission)
                .Select(role => new RoleResponse
                {
                    RoleId = role.RoleId,
                    RoleName = role.RoleName,
                    RolePermissions = role.Roles_Permissions.Select(rp => new RolePermissionResponse
                    {
                        PermissionId = rp.Permission.PermissionId,
                        PermissionName = rp.Permission.PermissionName
                    }).ToList()
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}