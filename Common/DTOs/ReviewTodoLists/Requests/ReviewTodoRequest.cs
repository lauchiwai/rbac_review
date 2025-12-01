namespace Common.DTOs.ReviewTodoLists.Requests;

public class ReviewTodoRequest
{
    public int TodoId { get; set; }

    public int ReviewerUserId { get; set; }

    public int? NextReviewerUserId { get; set; }

    public string Action { get; set; } = null!; // "approve", "reject", "return", "complete"

    public string? Comment { get; set; }

    public int? NextReviewLevel { get; set; } 
}