namespace Common.DTOs.ReviewTodoLists.Responses;

public class ReviewHistoryItem
{
    
    public int ReviewId { get; set; }

    public int TodoId { get; set; }

    public string Action { get; set; } = null!;

    public int ReviewerUserId { get; set; }

    public int? NextReviewerUserId { get; set; }

    public string? Comment { get; set; }

    public DateTime ReviewedAt { get; set; }

    public string? PreviousStatus { get; set; }

    public string? NewStatus { get; set; }
}