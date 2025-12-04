using Common.DTOs;
using Common.DTOs.Users.Requests;
using Common.DTOs.Users.Responses;

namespace Services.Interfaces;

public interface IUserService
{
    Task<ResultDto<CreateUserResponse>> CreateUserAsync();

    Task<ResultDto> DeleteUserAsync(int userId);

    Task<ResultDto> AssignRoleToUserAsync(AssignRoleToUserRequest request);

    Task<ResultDto> RemoveRoleFromUserAsync(RemoveRoleFromUserRequest request);

    Task<ResultDto<UserRolesResponse>> GetUserRolesAsync(int userId);
}