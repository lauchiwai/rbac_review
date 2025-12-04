namespace Common.DTOs.Review.Response;

public class TodoReviewHistoryResponse
{
    public DateTime ReviewedAt { get; set; }
    public string ReviewerUserName { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string? Comment { get; set; }
    public string? PreviousStatus { get; set; }
    public string? NewStatus { get; set; }
    public string StageName { get; set; } = null!;
}