using Common.DTOs;
using Common.DTOs.Template.Requests;
using Common.DTOs.Template.Response;
using Common.DTOs.Todo.Requests;
using Common.DTOs.Todo.Response;
using Common.DTOs.Transition.Requests;
using Common.DTOs.Transition.Response;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodoController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    [HttpPost("CreateTodo")]
    public async Task<ActionResult<ResultDto<TodoCreateResponse>>> CreateTodo(
        [FromBody] TodoCreateRequest request)
    {
        var result = await _todoService.CreateTodoAsync(request);

        return StatusCode((int)result.StatusCode, result);
    }
}