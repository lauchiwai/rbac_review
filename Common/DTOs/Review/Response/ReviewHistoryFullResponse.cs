using Common.DTOs.Review.Requests;

namespace Common.DTOs.Review.Response;

public class ReviewHistoryFullResponse
{
    public int TodoListId { get; set; }
    public string Title { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string CreatedByUserName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string TemplateName { get; set; } = null!;
    public List<ReviewTimelineItemResponse> Timeline { get; set; } = new();
    public ReviewSummaryResponse Summary { get; set; } = new();
}


public class ReviewTimelineItemResponse
{
    public DateTime Time { get; set; }
    public string Stage { get; set; } = null!;
    public string ReviewerUserName { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string ResultStatus { get; set; } = null!;
    public string? Comment { get; set; }
    public string ActionDisplayName { get; set; } = null!;
    public string StatusDisplayName { get; set; } = null!;
}