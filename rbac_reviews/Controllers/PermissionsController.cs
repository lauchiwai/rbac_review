using Common.DTOs;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace rbac_reviews.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionsService _permissionsService;

        public PermissionsController(IPermissionsService permissionsService)
        {
            _permissionsService = permissionsService;
        }

        [HttpGet("get-all")]
        public async Task<ActionResult<ResultDto<IEnumerable<Permissions>>>> GetAllPermissions()
        {
            var result = await _permissionsService.GetAllPermissionsAsync();
            return Ok(result);
        }

        [HttpGet("get/{id}")]
        public async Task<ActionResult<ResultDto<Permissions>>> GetPermissionById(int id)
        {
            var result = await _permissionsService.GetPermissionByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<ResultDto<Permissions>>> CreatePermission([FromBody] string permissionName)
        {
            var result = await _permissionsService.CreatePermissionAsync(permissionName);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<ResultDto>> UpdatePermission(int id, [FromBody] Permissions permission)
        {
            var result = await _permissionsService.UpdatePermissionAsync(permission);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<ResultDto>> DeletePermission(int id)
        {
            var result = await _permissionsService.DeletePermissionAsync(id);
            return Ok(result);
        }
    }
}