using Common.DTOs;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace rbac_reviews.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRolesService _rolesService;

    public RolesController(IRolesService rolesService)
    {
        _rolesService = rolesService;
    }

    [HttpGet("GetAllRoles")]
    public async Task<ActionResult<ResultDto<IEnumerable<Roles>>>> GetAllRoles()
    {
        var result = await _rolesService.GetAllRolesAsync();
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("CreateRole")]
    public async Task<ActionResult<ResultDto<Roles>>> CreateRole([FromBody] string roleName)
    {
        var result = await _rolesService.CreateRoleAsync(roleName);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPut("UpdateRole")]
    public async Task<ActionResult<ResultDto>> UpdateRole([FromBody] Roles role)
    {
        var result = await _rolesService.UpdateRoleAsync(role);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpDelete("DeleteRole/{id}")]
    public async Task<ActionResult<ResultDto>> DeleteRole(int id)
    {
        var result = await _rolesService.DeleteRoleAsync(id);
        return StatusCode((int)result.StatusCode, result);
    }
}