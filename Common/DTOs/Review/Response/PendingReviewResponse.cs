using Common.DTOs.Review.Requests;

namespace Common.DTOs.Review.Response;

public class PendingReviewResponse
{
    public int TodoListId { get; set; }
    public string Title { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string CreatedByUserName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string CurrentStageName { get; set; } = null!;
    public string CurrentReviewerUserName { get; set; } = null!;
    public List<AvailableActionResponse> AvailableActions { get; set; } = new();
}