using Common.DTOs;
using Common.DTOs.TodoLists.Requests;
using Common.DTOs.TodoLists.Responses;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace rbac_reviews.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodosController : ControllerBase
    {
        private readonly ITodoListsService _todoService;

        public TodosController(ITodoListsService todoService)
        {
            _todoService = todoService;
        }

        [HttpGet("get-all")]
        public async Task<ActionResult<ResultDto<List<TodoViewModel>>>> GetAllTodos([FromQuery] int currentUserRoleId)
        {
            if (currentUserRoleId <= 0)
            {
                return BadRequest(ResultDto<List<TodoViewModel>>.BadRequest("Invalid CurrentUserRoleId"));
            }

            var request = new GetTodosRequest { CurrentUserRoleId = currentUserRoleId };
            var result = await _todoService.GetAllTodosAsync(request);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpGet("get/{id}")]
        public async Task<ActionResult<ResultDto<TodoViewModel>>> GetTodoById(int id, [FromQuery] int currentUserRoleId)
        {
            if (id <= 0 || currentUserRoleId <= 0)
            {
                return BadRequest(ResultDto<TodoViewModel>.BadRequest("Invalid ID or CurrentUserRoleId"));
            }

            var result = await _todoService.GetTodoAsync(id, currentUserRoleId);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<ResultDto<TodoViewModel>>> CreateTodo([FromBody] CreateTodoRequest request)
        {
            if (request?.CreatedByRoleId <= 0)
            {
                return BadRequest(ResultDto<TodoViewModel>.BadRequest("Invalid CreatedByRoleId"));
            }

            var result = await _todoService.CreateTodoAsync(request);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<ResultDto<TodoViewModel>>> UpdateTodo(int id, [FromBody] UpdateTodoRequest request)
        {
            if (id <= 0 || request?.CurrentUserRoleId <= 0)
            {
                return BadRequest(ResultDto<TodoViewModel>.BadRequest("Invalid ID or CurrentUserRoleId"));
            }

            if (id != request.TodoId)
            {
                return BadRequest(ResultDto<TodoViewModel>.BadRequest("URL ID and request body TodoId do not match"));
            }

            var result = await _todoService.UpdateTodoAsync(request);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<ResultDto>> DeleteTodo(int id, [FromQuery] int currentUserRoleId)
        {
            if (id <= 0 || currentUserRoleId <= 0)
            {
                return BadRequest(ResultDto.BadRequest("Invalid ID or currentUserRoleId"));
            }

            var result = await _todoService.DeleteTodoAsync(id, currentUserRoleId);
            return StatusCode((int)result.StatusCode, result);
        }
    }
}
