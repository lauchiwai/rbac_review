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

    [HttpPost("roles/{roleId}/permissions/{permissionId}")]
    public async Task<ActionResult<ResultDto>> AssignPermissionToRole(int roleId, int permissionId)
    {
        var request = new AssignPermissionRequest
        {
            RoleId = roleId,
            PermissionId = permissionId
        };
        var result = await _rbacService.AssignPermissionAsync(request);
        return Ok(result);
    }

    [HttpDelete("roles/{roleId}/permissions/{permissionId}")]
    public async Task<ActionResult<ResultDto>> RemovePermissionFromRole(int roleId, int permissionId)
    {
        var request = new RemovePermissionRequest
        {
            RoleId = roleId,
            PermissionId = permissionId
        };
        var result = await _rbacService.RemovePermissionAsync(request);
        return Ok(result);
    }

    [HttpGet("roles/{roleId}/permissions")]
    public async Task<ActionResult<ResultDto<RolePermissionsResponse>>> GetRolePermissions(int roleId)
    {
        var result = await _rbacService.GetRolePermissionsAsync(roleId);
        return Ok(result);
    }

    [HttpGet("permissions/{permissionId}/roles")]
    public async Task<ActionResult<ResultDto<PermissionRolesResponse>>> GetRolesWithPermission(int permissionId)
    {
        var result = await _rbacService.GetRolesByPermissionAsync(permissionId);
        return Ok(result);
    }
}