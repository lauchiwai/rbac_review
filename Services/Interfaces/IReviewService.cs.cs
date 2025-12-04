using Common.DTOs;
using Common.DTOs.Review.Requests;
using Common.DTOs.Review.Response;

namespace Services.Interfaces;

public interface IReviewService
{
    Task<ResultDto<List<PendingReviewResponse>>> GetPendingReviewsAsync(int userId);

    Task<ResultDto<TodoDetailResponse>> GetTodoDetailAsync(int userId, int todoId);

    Task<ResultDto<ReviewActionResponse>> ExecuteReviewActionAsync(ReviewActionRequest request);

    Task<ResultDto<ReviewHistoryFullResponse>> GetReviewHistoryAsync(int userId, int todoId);
}