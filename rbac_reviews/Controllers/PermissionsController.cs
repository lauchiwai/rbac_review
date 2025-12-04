using Common.DTOs;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace rbac_reviews.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionsService _permissionsService;

    public PermissionsController(IPermissionsService permissionsService)
    {
        _permissionsService = permissionsService;
    }

    [HttpGet("GetAllPermissions")]
    public async Task<ActionResult<ResultDto<IEnumerable<Permissions>>>> GetAllPermissions()
    {
        var result = await _permissionsService.GetAllPermissionsAsync();
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpGet("GetPermissionById/{id}")]
    public async Task<ActionResult<ResultDto<Permissions>>> GetPermissionById(int id)
    {
        var result = await _permissionsService.GetPermissionByIdAsync(id);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("CreatePermission")]
    public async Task<ActionResult<ResultDto<Permissions>>> CreatePermission([FromBody] string permissionName)
    {
        var result = await _permissionsService.CreatePermissionAsync(permissionName);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPut("UpdatePermission/{id}")]
    public async Task<ActionResult<ResultDto>> UpdatePermission(int id, [FromBody] Permissions permission)
    {
        var result = await _permissionsService.UpdatePermissionAsync(permission);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpDelete("DeletePermission/{id}")]
    public async Task<ActionResult<ResultDto>> DeletePermission(int id)
    {
        var result = await _permissionsService.DeletePermissionAsync(id);
        return StatusCode((int)result.StatusCode, result);
    }
}