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

    [HttpPost("CreateUser")]
    public async Task<ActionResult<ResultDto<CreateUserResponse>>> CreateUser()
    {
        var result = await _userService.CreateUserAsync();
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpDelete("DeleteUser/{userId}")]
    public async Task<ActionResult<ResultDto>> DeleteUser(int userId)
    {
        var result = await _userService.DeleteUserAsync(userId);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("AssignRoleToUser")]
    public async Task<ActionResult<ResultDto>> AssignRoleToUser([FromBody] AssignRoleToUserRequest request)
    {
        var result = await _userService.AssignRoleToUserAsync(request);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpDelete("RemoveRoleFromUser/users/{userId}/roles/{roleId}")]
    public async Task<ActionResult<ResultDto>> RemoveRoleFromUser(int userId, int roleId)
    {
        var request = new RemoveRoleFromUserRequest
        {
            UserId = userId,
            RoleId = roleId
        };
        var result = await _userService.RemoveRoleFromUserAsync(request);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpGet("GetUserRoles/{userId}")]
    public async Task<ActionResult<ResultDto<UserRolesResponse>>> GetUserRoles(int userId)
    {
        var result = await _userService.GetUserRolesAsync(userId);
        return StatusCode((int)result.StatusCode, result);
    }
}