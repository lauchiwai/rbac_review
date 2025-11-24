namespace Common.DTOs.ReviewTodoLists.Responses;

public class ReviewHistoryItem
{
    public int TodoId { get; set; }

    public string Action { get; set; } = null!;

    public int ReviewerRoleId { get; set; }

    public string? Comment { get; set; }

    public DateTime ReviewedAt { get; set; }

    public string? PreviousStatus { get; set; }

    public string? NewStatus { get; set; }
}