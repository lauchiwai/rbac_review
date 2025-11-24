using Common.DTOs;
using Common.DTOs.ReviewTodoLists.Requests;
using Common.DTOs.ReviewTodoLists.Responses;
using Common.DTOs.TodoLists.Responses;

namespace Services.Interfaces;

public interface ITodoReviewService
{
    Task<ResultDto<List<TodoWithReviewHistoryViewModel>>> GetReviewTodosAsync(GetReviewTodosRequest request);

    Task<ResultDto<ReviewHistoryItem>> ReviewTodoAsync(ReviewTodoRequest request);

    Task<ResultDto<List<ReviewHistoryItem>>> GetReviewHistoryAsync(int todoId, int currentUserRoleId);
}
