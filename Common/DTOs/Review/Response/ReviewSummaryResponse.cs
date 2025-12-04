namespace Common.DTOs.Review.Response;

public class ReviewSummaryResponse
{
    public int TotalReviews { get; set; }
    public string FirstReviewDate { get; set; } = null!;
    public string LastReviewDate { get; set; } = null!;
    public string TotalDuration { get; set; } = null!;
    public string CurrentStatus { get; set; } = null!;
    public int ApprovalCount { get; set; }
    public int ReturnCount { get; set; }
    public int RejectCount { get; set; }
}