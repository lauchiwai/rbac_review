namespace Common.DTOs.Review.Response;

public class ReviewActionResponse
{
    public int TodoListId { get; set; }
    public string Title { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string PreviousStatus { get; set; } = null!;
    public string Action { get; set; } = null!;
    public int ReviewId { get; set; }
    public DateTime ReviewedAt { get; set; }
    public string? Comment { get; set; }
    public string CurrentStageName { get; set; } = null!;
    public string CurrentReviewerUserName { get; set; } = null!;
    public string NextStageName { get; set; } = null!;
    public bool IsCompleted { get; set; }
}