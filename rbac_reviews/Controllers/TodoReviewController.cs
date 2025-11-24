using Common.DTOs;
using Common.DTOs.ReviewTodoLists.Requests;
using Common.DTOs.ReviewTodoLists.Responses;
using Common.DTOs.TodoLists.Responses;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace rbac_reviews.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoReviewsController : ControllerBase
    {
        private readonly ITodoReviewService _todoReviewService;

        public TodoReviewsController(ITodoReviewService todoReviewService)
        {
            _todoReviewService = todoReviewService;
        }

        [HttpGet("get-review-todos")]
        public async Task<ActionResult<ResultDto<List<TodoViewModel>>>> GetReviewTodos([FromQuery] int currentUserRoleId, [FromQuery] string? status, [FromQuery] int? reviewLevel)
        {
            if (currentUserRoleId <= 0)
            {
                return BadRequest(ResultDto<List<TodoViewModel>>.BadRequest("Invalid CurrentUserRoleId"));
            }

            var request = new GetReviewTodosRequest
            {
                CurrentUserRoleId = currentUserRoleId,
                Status = status,
                ReviewLevel = reviewLevel
            };

            var result = await _todoReviewService.GetReviewTodosAsync(request);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("review")]
        public async Task<ActionResult<ResultDto<ReviewHistoryItem>>> ReviewTodo([FromBody] ReviewTodoRequest request)
        {
            if (request?.ReviewerRoleId <= 0)
            {
                return BadRequest(ResultDto<ReviewHistoryItem>.BadRequest("Invalid ReviewerRoleId"));
            }

            if (request?.TodoId <= 0)
            {
                return BadRequest(ResultDto<ReviewHistoryItem>.BadRequest("Invalid TodoId"));
            }

            if (string.IsNullOrEmpty(request.Action))
            {
                return BadRequest(ResultDto<ReviewHistoryItem>.BadRequest("Action is required"));
            }

            var result = await _todoReviewService.ReviewTodoAsync(request);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpGet("review-history/{todoId}")]
        public async Task<ActionResult<ResultDto<List<ReviewHistoryItem>>>> GetReviewHistory(int todoId, [FromQuery] int currentUserRoleId)
        {
            if (todoId <= 0 || currentUserRoleId <= 0)
            {
                return BadRequest(ResultDto<List<ReviewHistoryItem>>.BadRequest("Invalid TodoId or CurrentUserRoleId"));
            }

            var result = await _todoReviewService.GetReviewHistoryAsync(todoId, currentUserRoleId);
            return StatusCode((int)result.StatusCode, result);
        }
    }
}
