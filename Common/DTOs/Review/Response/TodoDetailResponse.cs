using Common.DTOs.Review.Requests;
using Common.DTOs.Stage.Response;

namespace Common.DTOs.Review.Response;

public class TodoDetailResponse
{
    public int TodoListId { get; set; }
    public string Title { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string CreatedByUserName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string CurrentStageName { get; set; } = null!;
    public string CurrentReviewerUserName { get; set; } = null!;
    public List<TodoReviewHistoryResponse> ReviewHistory { get; set; } = new();
    public List<AvailableActionResponse> AvailableActions { get; set; } = new();
    public string TemplateName { get; set; } = null!;
    public List<StageInfoResponse> AllStages { get; set; } = new();
}