using Common.DTOs;
using Common.DTOs.Rbac.Requests;
using Common.DTOs.Rbac.Responses;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace rbac_reviews.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RbacController : ControllerBase
{
    private readonly IRbacService _rbacService;

    public RbacController(IRbacService rbacService)
    {
        _rbacService = rbacService;
    }

    [HttpPost("AssignPermissionToRole")]
    public async Task<ActionResult<ResultDto>> AssignPermissionToRole([FromBody] AssignPermissionRequest request)
    {
        var result = await _rbacService.AssignPermissionAsync(request);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpDelete("RemovePermissionFromRole/roles/{roleId}/permissions/{permissionId}")]
    public async Task<ActionResult<ResultDto>> RemovePermissionFromRole(int roleId, int permissionId)
    {
        var request = new RemovePermissionRequest
        {
            RoleId = roleId,
            PermissionId = permissionId
        };
        var result = await _rbacService.RemovePermissionAsync(request);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpGet("GetRolePermissions/roles/{roleId}")]
    public async Task<ActionResult<ResultDto<RolePermissionsResponse>>> GetRolePermissions(int roleId)
    {
        var result = await _rbacService.GetRolePermissionsAsync(roleId);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpGet("GetRolesWithPermission/permissions/{permissionId}")]
    public async Task<ActionResult<ResultDto<PermissionRolesResponse>>> GetRolesWithPermission(int permissionId)
    {
        var result = await _rbacService.GetRolesByPermissionAsync(permissionId);
        return StatusCode((int)result.StatusCode, result);
    }
}