using Common.DTOs;
using Common.DTOs.Users.Requests;
using Common.DTOs.Users.Responses;
using Common.Models;
using Repositories.MyRepository;
using Services.Interfaces;
using System.Linq.Expressions;

namespace Services.Implementations;

public class UserService : IUserService
{
    private readonly IRepository<Users> _userRepository;
    private readonly IRepository<Users_Roles> _userRoleRepository;
    private readonly IRepository<Roles> _roleRepository;

    public UserService(
        IRepository<Users> userRepository,
        IRepository<Users_Roles> userRoleRepository,
        IRepository<Roles> roleRepository)
    {
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
    }

    public async Task<ResultDto<CreateUserResponse>> CreateUserAsync()
    {
        try
        {
            var user = new Users
            {
                CreatedAt = DateTime.Now
            };

            await _userRepository.AddAsync(user);

            var response = new CreateUserResponse
            {
                UserId = user.UserId,
                CreatedAt = user.CreatedAt
            };

            return ResultDto<CreateUserResponse>.Success(response, "User created successfully");
        }
        catch (Exception ex)
        {
            return ResultDto<CreateUserResponse>.Failure($"Error creating user: {ex.Message}");
        }
    }

    public async Task<ResultDto> DeleteUserAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return ResultDto.Failure($"User with ID {userId} not found");

            await _userRepository.DeleteAsync(user);
            return ResultDto.Success("User deleted successfully");
        }
        catch (Exception ex)
        {
            return ResultDto.Failure($"Error deleting user: {ex.Message}");
        }
    }

    public async Task<ResultDto> AssignRoleToUserAsync(AssignRoleToUserRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            var role = await _roleRepository.GetByIdAsync(request.RoleId);

            if (user == null)
                return ResultDto.Failure($"User with ID {request.UserId} not found");

            if (role == null)
                return ResultDto.Failure($"Role with ID {request.RoleId} not found");

            Expression<Func<Users_Roles, bool>> predicate = ur =>
                ur.UserId == request.UserId && ur.RoleId == request.RoleId;

            var existing = await _userRoleRepository.FindAsync(predicate);
            if (existing.Any())
                return ResultDto.Failure("Role is already assigned to this user");

            var userRole = new Users_Roles
            {
                UserId = request.UserId,
                RoleId = request.RoleId
            };

            await _userRoleRepository.AddAsync(userRole);
            return ResultDto.Success("Role assigned to user successfully");
        }
        catch (Exception ex)
        {
            return ResultDto.Failure($"Error assigning role to user: {ex.Message}");
        }
    }

    public async Task<ResultDto> RemoveRoleFromUserAsync(RemoveRoleFromUserRequest request)
    {
        try
        {
            Expression<Func<Users_Roles, bool>> predicate = ur =>
                ur.UserId == request.UserId && ur.RoleId == request.RoleId;

            var userRoles = await _userRoleRepository.FindAsync(predicate);
            var userRole = userRoles.FirstOrDefault();

            if (userRole == null)
                return ResultDto.Failure("Role is not assigned to this user");

            await _userRoleRepository.DeleteAsync(userRole);
            return ResultDto.Success("Role removed from user successfully");
        }
        catch (Exception ex)
        {
            return ResultDto.Failure($"Error removing role: {ex.Message}");
        }
    }

    public async Task<ResultDto<UserRolesResponse>> GetUserRolesAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return ResultDto<UserRolesResponse>.Failure($"User with ID {userId} not found");

            Expression<Func<Users_Roles, bool>> predicate = ur => ur.UserId == userId;
            var userRoles = await _userRoleRepository.FindAsync(predicate);

            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
            var roles = new List<UserRoleViewModel>();

            foreach (var roleId in roleIds)
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role != null)
                {
                    roles.Add(new UserRoleViewModel
                    {
                        RoleId = role.RoleId,
                        RoleName = role.RoleName
                    });
                }
            }

            var response = new UserRolesResponse
            {
                UserId = user.UserId,
                Roles = roles
            };

            return ResultDto<UserRolesResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return ResultDto<UserRolesResponse>.Failure($"Error retrieving user roles: {ex.Message}");
        }
    }
}