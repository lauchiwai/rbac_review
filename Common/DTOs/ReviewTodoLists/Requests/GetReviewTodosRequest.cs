namespace Common.DTOs.ReviewTodoLists.Requests;

public class GetReviewTodosRequest
{
    public int CurrentUserId { get; set; }

    public string? Status { get; set; } 

    public int? ReviewLevel { get; set; } 
}
