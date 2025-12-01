using Common.DTOs;
using Common.DTOs.Users.Requests;
using Common.DTOs.Users.Responses;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace rbac_reviews.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("create")]
    public async Task<ActionResult<ResultDto<CreateUserResponse>>> CreateUser()
    {
        var result = await _userService.CreateUserAsync();
        return Ok(result);
    }

    [HttpDelete("delete/{userId}")]
    public async Task<ActionResult<ResultDto>> DeleteUser(int userId)
    {
        var result = await _userService.DeleteUserAsync(userId);
        return Ok(result);
    }

    [HttpPost("{userId}/roles/{roleId}")]
    public async Task<ActionResult<ResultDto>> AssignRoleToUser(int userId, int roleId)
    {
        var request = new AssignRoleToUserRequest
        {
            UserId = userId,
            RoleId = roleId
        };
        var result = await _userService.AssignRoleToUserAsync(request);
        return Ok(result);
    }

    [HttpDelete("{userId}/roles/{roleId}")]
    public async Task<ActionResult<ResultDto>> RemoveRoleFromUser(int userId, int roleId)
    {
        var request = new RemoveRoleFromUserRequest
        {
            UserId = userId,
            RoleId = roleId
        };
        var result = await _userService.RemoveRoleFromUserAsync(request);
        return Ok(result);
    }

    [HttpGet("roles/{userId}")]
    public async Task<ActionResult<ResultDto<UserRolesResponse>>> GetUserRoles(int userId)
    {
        var result = await _userService.GetUserRolesAsync(userId);
        return Ok(result);
    }

    [HttpGet("roles/{roleId}/users")]
    public async Task<ActionResult<ResultDto<RoleUsersResponse>>> GetUsersByRole(int roleId)
    {
        var result = await _userService.GetUsersByRoleAsync(roleId);
        return Ok(result);
    }

    [HttpGet("{userId}/has-role/{roleId}")]
    public async Task<ActionResult<ResultDto<bool>>> HasRole(int userId, int roleId)
    {
        var result = await _userService.HasRoleAsync(userId, roleId);
        return Ok(result);
    }
}