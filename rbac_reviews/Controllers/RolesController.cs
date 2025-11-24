using Common.DTOs;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace rbac_reviews.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRolesService _rolesService;

        public RolesController(IRolesService rolesService)
        {
            _rolesService = rolesService;
        }

        [HttpGet("get-all")]
        public async Task<ActionResult<ResultDto<IEnumerable<Roles>>>> GetAllRoles()
        {
            var result = await _rolesService.GetAllRolesAsync();
            return Ok(result);
        }

        [HttpGet("get/{id}")]
        public async Task<ActionResult<ResultDto<Roles>>> GetRoleById(int id)
        {
            var result = await _rolesService.GetRoleByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<ResultDto<Roles>>> CreateRole([FromBody] string roleName)
        {
            var result = await _rolesService.CreateRoleAsync(roleName);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<ResultDto>> UpdateRole(int id, [FromBody] Roles role)
        {
            var result = await _rolesService.UpdateRoleAsync(role);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<ResultDto>> DeleteRole(int id)
        {
            var result = await _rolesService.DeleteRoleAsync(id);
            return Ok(result);
        }
    }
}