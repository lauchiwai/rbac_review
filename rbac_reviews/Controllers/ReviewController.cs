using Common.DTOs;
using Common.DTOs.Review.Requests;
using Common.DTOs.Review.Response;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("GetPendingReviews/{UserId}")]
    public async Task<ActionResult<ResultDto<List<PendingReviewResponse>>>> GetPendingReviews(
       int userId)
    {
        var result = await _reviewService.GetPendingReviewsAsync(userId);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpGet("GetTodoDetail/users/{userId}/todos/{todoId}")]
    public async Task<ActionResult<ResultDto<TodoDetailResponse>>> GetTodoDetail(
        int userId, int todoId)
    {
        var result = await _reviewService.GetTodoDetailAsync(userId, todoId);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("ExecuteReviewAction")]
    public async Task<ActionResult<ResultDto<ReviewActionResponse>>> ExecuteReviewAction(
        [FromBody] ReviewActionRequest request)
    {
        var result = await _reviewService.ExecuteReviewActionAsync(request);
        return StatusCode((int)result.StatusCode, result);
    }


    [HttpPost("GetReviewHistory/users/{userId}/todos/{todoId}")]
    public async Task<ActionResult<ResultDto<ReviewHistoryFullResponse>>> GetReviewHistory(
        int userId, int todoId)
    {
        var result = await _reviewService.GetReviewHistoryAsync(userId, todoId);
        return StatusCode((int)result.StatusCode, result);
    }
}